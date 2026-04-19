// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Extensions;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public unsafe partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private delegate void MipsEmitter(ILGenerator il, MipsInstruction inst, T pc);
    private delegate void MipsFloatEmitter(ILGenerator il, FloatInstruction inst, T pc);

    // Main tables
    private readonly MipsEmitter[] _opCodeTable = new MipsEmitter[64];
    private readonly MipsEmitter[] _specialTable = new MipsEmitter[64];
    private readonly MipsEmitter[] _special2Table = new MipsEmitter[64];
    private readonly MipsEmitter[] _regImmTable = new MipsEmitter[32];

    // CoProcessor tables
    private readonly MipsFloatEmitter[] _coProc1RSTable = new MipsFloatEmitter[32];
    private readonly MipsFloatEmitter[][] _floatFuncTables;

    private readonly MethodInfo _getMemoryMethod;
    private readonly MethodInfo _clzMethod;
    private readonly Dictionary<Type, MethodInfo> _readMethods = [];
    private readonly Dictionary<Type, MethodInfo> _writeMethods = [];
    private readonly Dictionary<Type, MethodInfo> _multiplyMethod = [];
    private readonly Dictionary<Type, MethodInfo> _castDownMethods = [];
    private readonly Dictionary<Type, MethodInfo> _rightShiftMethods = [];
    private readonly MipsJitCpu<T> _cpu;

    private readonly HashSet<MipsGpRegister> _loadRegs = [];
    private readonly HashSet<MipsGpRegister> _storeRegs = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitCompiler{T}"/> class.
    /// </summary>
    public MipsJitCompiler(MipsJitCpu<T> cpu)
    {
        _cpu = cpu;

        var getMemoryMethod = _cpu.GetType().GetProperty(nameof(MipsJitCpu<>.Memory))?.GetGetMethod();
        Guard.IsNotNull(getMemoryMethod);
        _getMemoryMethod = getMemoryMethod;

        var clzMethod = typeof(BitOperations).GetMethod(nameof(BitOperations.LeadingZeroCount), [typeof(uint)]);
        Guard.IsNotNull(clzMethod);
        _clzMethod = clzMethod;

        Type[] readWritetypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint)];
        foreach (var type in readWritetypes)
        {
            _readMethods[type] = typeof(MemorySystem)
                .GetMethods()
                .First(m => m.Name == nameof(MemorySystem.Read) && m.IsGenericMethod && m.GetParameters().Length == 1)
                .MakeGenericMethod(type);
            _writeMethods[type] = typeof(MemorySystem)
                .GetMethods()
                .First(m => m.Name == nameof(MemorySystem.Write) && m.IsGenericMethod && m.GetParameters().Length == 2)
                .MakeGenericMethod(type);
        }

        Type[] multiplyTypes = [typeof(int), typeof(uint), typeof(long), typeof(ulong)];
        foreach (var type in multiplyTypes)
        {
            var method = typeof(Math).GetMethod(nameof(Math.BigMul), [type, type]);
            Guard.IsNotNull(method);
            _multiplyMethod[type] = method;
        }

        (Type, Type)[] bigTypePairs = [(typeof(Int128), typeof(long)), (typeof(UInt128), typeof(ulong))];
        foreach (var (type, pair) in bigTypePairs)
        {
            var castDownMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Explicit" && m.ReturnType == pair);
            var rightShiftMethod = type.GetMethod("op_RightShift", [type, typeof(int)]);
            Guard.IsNotNull(castDownMethod);
            Guard.IsNotNull(rightShiftMethod);
            _castDownMethods[type] = castDownMethod;
            _rightShiftMethods[type] = rightShiftMethod;
        }

        var formatCount = _cpu.Config.Version.Is64Bit() ? 4 : 3;
        _floatFuncTables = new MipsFloatEmitter[formatCount][];

        InitTables(_cpu.Config);
    }

    /// <summary>
    /// Compiles a block of MIPS code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public MipsJitBlock<T> CompileBlock(T startPc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T>), typeof(MipsTrap).MakeByRefType()];
        var method = new DynamicMethod($"Block_0x{startPc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();

        var endPc = DiscoverBlock(startPc);
        ScanRegisterUsage(startPc, endPc);
        EmitSetupLocalRegisters(il);

        T currentPc = startPc;

        while (currentPc < endPc)
        {
            CompileInstruction(il, Fetch(currentPc), currentPc);
            currentPc += T.CreateTruncating(4);
        }

        var @delegate = (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
        return new(@delegate, int.CreateTruncating(endPc - startPc));
    }

    /// <summary>
    /// Compiles a single instruction as a cli dynamic method.
    /// </summary>
    public MipsBlockDelegate<T> CompileLoneInstruction(MipsInstruction inst, T pc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T>), typeof(MipsTrap).MakeByRefType()];
        var method = new DynamicMethod($"Insert_0x{pc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();

        ScanRegisterUsage(pc, pc);
        LogRegisterUsage(inst);
        EmitSetupLocalRegisters(il);
        CompileInstruction(il, inst, pc);

        if (!IsControlFlow(inst))
        {
            EmitRet(il, pc + T.CreateTruncating(4));
        }

        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }

    private void CompileInstruction(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emitter = _opCodeTable[(int)inst.OpCode];
        emitter(il, inst, pc);
    }

    private void DispatchSpecial(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emmiter = _specialTable[(int)inst.FuncCode];
        emmiter(il, inst, pc);
    }

    private void DispatchSpecial2(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emmiter = _special2Table[(int)inst.FuncCode];
        emmiter(il, inst, pc);
    }

    private void DispatchRegImm(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emmiter = _regImmTable[(int)inst.RTFuncCode];
        emmiter(il, inst, pc);
    }

    private void Shift<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister<TData>(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, (int)inst.ShiftAmount);
            il.Emit(ilOpCode);
        });
    }

    private void ShiftPlus32<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister<TData>(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, inst.ShiftAmount + 32);
            il.Emit(ilOpCode);
        });
    }

    private void ShiftVar<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister<TData>(il, inst.RT); // Value to shift
            EmitLoadRegister<TData>(il, inst.RS); // Shift amount from register

            // Ensure the shift amount is treated as an int for the IL stack
            if (typeof(TData) == typeof(ulong))
                il.Emit(OpCodes.Conv_I4);

            il.Emit(ilOpCode);
        });
    }

    private void AluR<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister<TData>(il, inst.RS);
            EmitLoadRegister<TData>(il, inst.RT);
            il.Emit(ilOpCode);

            if (followUp.HasValue)
            {
                il.Emit(followUp.Value);
            }

            // Convert to T if neccesary
            if (sizeof(TData) != sizeof(T))
                EmitConv(il, IsSigned<TData>());
        });
    }

    private void CheckedAluR<TData>(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode, bool isSubtraction)
        where TData : unmanaged, INumber<TData>
    {
        Label noOverflow = il.DefineLabel();

        // Load RS into local
        EmitLoadRegister<TData>(il, inst.RS);
        LocalBuilder rs = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, rs);

        // Load RT into local
        EmitLoadRegister<TData>(il, inst.RT);
        LocalBuilder rt = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, rt);

        // Calculate
        il.Emit(OpCodes.Ldloc, rs);
        il.Emit(OpCodes.Ldloc, rt);
        il.Emit(ilOpCode);

        // Store result
        LocalBuilder result = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard<TData>(il, pc, rs, rt, result, noOverflow, isSubtraction);

        // Safe Path
        il.MarkLabel(noOverflow);
        EmitStoreRegister(il, inst.RD, () =>
        {
            il.Emit(OpCodes.Ldloc, result);
            if (sizeof(TData) != sizeof(T))
                EmitConv(il, Sign.Signed);
        });
    }

    private void AluI<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, bool signExtend = false)
        where TData : unmanaged, INumber<TData>
    {
        // Fetch the raw immediate from the instruction
        var rawImm = inst.Immediate;
        var extended = signExtend ? TData.CreateTruncating((long)rawImm) : TData.CreateTruncating((ulong)rawImm);

        EmitStoreRegister(il, inst.RT, () =>
        {
            EmitLoadRegister<TData>(il, inst.RS);
            EmitLoadConstant(il, extended);

            il.Emit(ilOpCode);
        });
    }

    private void CheckedAluI<TData>(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        Label noOverflow = il.DefineLabel();

        // Load RS into local
        EmitLoadRegister(il, inst.RS);
        LocalBuilder rs = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, rs);

        // Load Immediate into local (Sign-extended)
        EmitLoadConstant(il, TData.CreateTruncating(inst.Immediate));
        LocalBuilder imm = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, imm);

        // Calculate
        il.Emit(OpCodes.Ldloc, rs);
        il.Emit(OpCodes.Ldloc, imm);
        il.Emit(ilOpCode);
        LocalBuilder result = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard<TData>(il, pc, rs, imm, result, noOverflow);

        // Safe Path
        il.MarkLabel(noOverflow);
        EmitStoreRegister(il, inst.RT, () =>
        {
            il.Emit(OpCodes.Ldloc, result);
            if (sizeof(TData) != sizeof(T))
                EmitConv(il, Sign.Signed);
        });
    }

    private void MultR<TData, TLong>(ILGenerator il, MipsInstruction inst, int c = 0)
        where TData : unmanaged, INumber<TData>
        where TLong : unmanaged, INumber<TLong>
    {
        // Retrieve the rs/rt registers
        EmitLoadRegister<TData>(il, inst.RS);
        EmitLoadRegister<TData>(il, inst.RT);

        // Apply multiplication and store rsult as a local
        var localResult = il.DeclareLocal(typeof(TLong));
        il.Emit(OpCodes.Call, _multiplyMethod[typeof(TData)]);
        il.Emit(OpCodes.Stloc, localResult);

        int shiftAmount = sizeof(TData) * 8;
        bool bigLong = sizeof(TLong) > sizeof(long);

        // Store high
        EmitStoreRegister(il, MipsGpRegister.High, () =>
        {
            if (c is not 0)
            {
                EmitLoadRegister(il, MipsGpRegister.High);
            }

            il.Emit(OpCodes.Ldloc, localResult);
            EmitLoadConstant(il, shiftAmount);

            if (bigLong)
            {
                il.Emit(OpCodes.Call, _rightShiftMethods[typeof(TLong)]);
                il.Emit(OpCodes.Call, _castDownMethods[typeof(TLong)]);
            }
            else
            {
                il.Emit(OpCodes.Shr_Un);
                EmitConv<TData>(il);
            }

            if (c is not 0)
            {
                il.Emit(c is > 0 ? OpCodes.Add : OpCodes.Sub);
            }

            EmitConv(il, IsSigned<TData>());
        });

        // Store low
        EmitStoreRegister(il, MipsGpRegister.Low, () =>
        {
            if (c is not 0)
            {
                EmitLoadRegister(il, MipsGpRegister.Low);
            }

            il.Emit(OpCodes.Ldloc, localResult);

            if (bigLong)
            {
                il.Emit(OpCodes.Call, _castDownMethods[typeof(TLong)]);
            }
            else
            {
                EmitConv<TData>(il);
            }

            if (c is not 0)
            {
                il.Emit(c is > 0 ? OpCodes.Add : OpCodes.Sub);
            }

            EmitConv(il);
        });
    }

    private void DivR<TData>(ILGenerator il, MipsInstruction inst, bool signed)
        where TData : unmanaged, INumber<TData>
    {
        Label endDiv = il.DefineLabel();

        // Load operands into locals to keep stack predictable
        EmitLoadRegister(il, inst.RS);
        LocalBuilder rsLocal = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, rsLocal);
        EmitLoadRegister(il, inst.RT);
        LocalBuilder rtLocal = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, rtLocal);

        // Guard against Div-By-Zero
        il.Emit(OpCodes.Ldloc, rtLocal);
        EmitLoadConstant(il, TData.Zero);
        il.Emit(OpCodes.Beq, endDiv);

        // Calculate and store the remainder to High
        EmitStoreRegister(il, MipsGpRegister.High, () =>
        {
            il.Emit(OpCodes.Ldloc, rsLocal);
            il.Emit(OpCodes.Ldloc, rtLocal);
            il.Emit(signed ? OpCodes.Rem : OpCodes.Rem_Un);
            if (sizeof(TData) != sizeof(T))
            {
                EmitConv(il, signed ? Sign.Signed : Sign.Unsigned);
            }
        });

        // Calculate and store the quotient to low
        EmitStoreRegister(il, MipsGpRegister.Low, () =>
        {
            il.Emit(OpCodes.Ldloc, rsLocal);
            il.Emit(OpCodes.Ldloc, rtLocal);
            il.Emit(signed ? OpCodes.Div : OpCodes.Div_Un);
            if (sizeof(TData) != sizeof(T))
            {
                EmitConv(il, signed ? Sign.Signed : Sign.Unsigned);
            }
        });

        il.MarkLabel(endDiv);
    }

    private void Load<TData>(ILGenerator il, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        var addrVar = EmitLoadEffectiveAddress<TData>(il, inst, pc, MipsTrap.AddressErrorLoad);

        // Write Back to RT
        EmitStoreRegister(il, inst.RT, () =>
        {
            // Call Memory.Read<TData>(ulong)
            var readMethod = _readMethods[typeof(TData)];
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, _getMemoryMethod);
            il.Emit(OpCodes.Ldloc, addrVar);                // Arg 1: ulong addr
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Callvirt, readMethod);

            // Sign-Extension / Zero-Extension then convert to T
            EmitConv<TData>(il);
            EmitConv(il);
        });
    }

    private void Store<TData>(ILGenerator il, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        var addrVar = EmitLoadEffectiveAddress<TData>(il, inst, pc, MipsTrap.AddressErrorStore);

        // Call Memory.Write<TData>(ulong, TData)
        var writeMethod = _writeMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, _getMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);            // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        EmitLoadRegister(il, inst.RT);              // Arg 2: TData value (Truncate the RT register value)
        EmitConv<TData>(il);
        il.Emit(OpCodes.Callvirt, writeMethod);
    }

    private void Jump(ILGenerator il, MipsInstruction inst, T pc, bool link = false) => Jump(il, pc, link: link, pushAddress: il =>
    {
        EmitLoadConstant(il, T.CreateTruncating(inst.Address));
    });

    private void JumpR(ILGenerator il, MipsInstruction inst, T pc, bool link = false) => Jump(il, pc, link: link, pushAddress: il =>
    {
        EmitLoadRegister(il, inst.RS);
    });

    private void Jump(ILGenerator il, T pc, Action<ILGenerator> pushAddress, bool link = false)
    {
        bool delaySlots = !_cpu.Config.DisableDelaySlots;

        if (link)
        {
            // Store the Return Address ($ra = PC + 8)
            // We use +8 because +4 is the delay slot, and we want to return AFTER that.
            T returnAddr = pc + (delaySlots ? T.CreateTruncating(8) : T.CreateTruncating(4));
            EmitStoreRegister(il, MipsGpRegister.ReturnAddress, () => EmitLoadConstant(il, returnAddr));
        }

        // Handle the Delay Slot
        if (delaySlots)
        {
            T delaySlotPc = pc + T.CreateTruncating(4);
            EmitDelaySlot(il, delaySlotPc);
        }

        // Exit the block by returning the new PC
        EmitRet(il, pushAddress);
    }

    private void BranchCompareReg(ILGenerator il, MipsInstruction inst, T pc, OpCode conditionOpCode, bool likely = false)
        => Branch(il, inst, pc, conditionOpCode, likely: likely, pushOperands: il =>
        {
            EmitLoadRegister(il, inst.RS);
            EmitLoadRegister(il, inst.RT);
        });

    private void BranchCompareZero(ILGenerator il, MipsInstruction inst, T pc, OpCode conditionOpCode, bool likely = false)
        => Branch(il, inst, pc, conditionOpCode, likely: likely, pushOperands: il =>
        {
            EmitLoadRegister(il, inst.RS);
            EmitLoadConstant(il, T.Zero);
        });

    private void Branch(ILGenerator il, MipsInstruction inst, T pc, OpCode conditionOpCode, Action<ILGenerator> pushOperands, bool likely = false)
    {
        bool delaySlotsEnabled = !_cpu.Config.DisableDelaySlots;

        Label takeBranch = il.DefineLabel();

        // Evaluate the branch condition
        pushOperands(il);
        il.Emit(conditionOpCode, takeBranch);

        // Branch NOT taken
        if (delaySlotsEnabled && !likely)
        {
            EmitDelaySlot(il, pc + T.CreateTruncating(4));
        }

        // Branch NOT taken
        var nextPc = pc + (delaySlotsEnabled ? T.CreateTruncating(8) : T.CreateTruncating(4));
        EmitRet(il, nextPc);

        // Branch taken
        // In both Likely and Normal modes, the delay slot executes if the branch is taken.
        il.MarkLabel(takeBranch);

        if (delaySlotsEnabled)
        {
            EmitDelaySlot(il, pc + T.CreateTruncating(4));
        }

        T targetPc = pc + T.CreateTruncating(4) + T.CreateTruncating(inst.Offset);
        EmitRet(il, targetPc);
    }

    private void TrapCompareReg(ILGenerator il, MipsInstruction inst, T pc, OpCode invertedBranch) => ConditionalTrap(il, inst, pc, invertedBranch, il =>
    {
        EmitLoadRegister(il, inst.RS);
        EmitLoadRegister(il, inst.RT);
    });

    private void TrapCompareImmediate(ILGenerator il, MipsInstruction inst, T pc, OpCode invertedBranch) => ConditionalTrap(il, inst, pc, invertedBranch, il =>
    {
        EmitLoadRegister(il, inst.RS);
        EmitLoadConstant(il, T.CreateTruncating(inst.Immediate));
    });

    private void ConditionalTrap(ILGenerator il, MipsInstruction inst, T pc, OpCode invertedBranch, Action<ILGenerator> pushOperands)
    {
        Label noTrap = il.DefineLabel();

        // Evaluate the trap condition
        pushOperands(il);
        il.Emit(invertedBranch, noTrap);

        // DO trap
        EmitTrapRet(il, MipsTrap.Trap, pc);

        // Do NOT trap
        il.MarkLabel(noTrap);
        EmitRet(il, pc);
    }

    private void Trap(ILGenerator il, T pc, MipsTrap trap)
    {
        EmitTrapRet(il, trap, pc);
    }

    private void Move(ILGenerator il, MipsInstruction inst, OpCode invertedBranch)
    {
        Label noMove = il.DefineLabel();

        EmitLoadRegister(il, inst.RT);
        il.Emit(invertedBranch, noMove);

        // DO move
        EmitStoreRegister(il, inst.RD, () => EmitLoadRegister(il, inst.RS));

        // Do NOT move
        il.MarkLabel(noMove);
    }

    private void MoveFromTo(ILGenerator il, MipsGpRegister from, MipsGpRegister to)
    {
        // Can't writeback to $zero.
        // Skip as no-op
        if (to is MipsGpRegister.Zero)
            return;

        EmitStoreRegister(il, to, () => EmitLoadRegister(il, from));
    }

    private void Lui(ILGenerator il, MipsInstruction inst)
    {
        uint value = (uint)inst.Immediate << 16;

        EmitStoreRegister(il, inst.RT, () => EmitLoadConstant(il, T.CreateTruncating(value)));
    }

    private void MethodUnary<TData>(ILGenerator il, MipsInstruction inst, Action method)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister<TData>(il, inst.RS);

            method();

            if (sizeof(TData) != sizeof(T))
                EmitConv(il);
        });
    }
}
