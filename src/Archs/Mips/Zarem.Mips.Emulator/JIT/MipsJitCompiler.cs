// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Emulator.JIT;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public unsafe partial class MipsJitCompiler<T, TFloat> : JitCompiler<T, MipsGpRegister, MipsTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private delegate void MipsEmitter(ILGenerator il, MipsInstruction inst, T pc);

    private readonly MipsInstructionDecodeTable<MipsEmitter> _instructionTable;

    private readonly MethodInfo _clzMethod;
    private readonly Dictionary<Type, MethodInfo> _multiplyMethod = [];
    private readonly Dictionary<Type, MethodInfo> _castDownMethods = [];
    private readonly Dictionary<Type, MethodInfo> _rightShiftMethods = [];
    private readonly MethodInfo _handleAccessResultMethod;
    private readonly MipsJitCpu<T, TFloat> _cpu;

    private readonly HashSet<MipsGpRegister> _loadRegs = [];
    private readonly HashSet<MipsGpRegister> _storeRegs = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitCompiler{T, TFloat}"/> class.
    /// </summary>
    public MipsJitCompiler(MipsJitCpu<T, TFloat> cpu) : base(cpu)
    {
        _cpu = cpu;

        _instructionTable = new MipsInstructionDecodeTable<MipsEmitter>(ReservedInstruction);

        var clzMethod = typeof(BitOperations).GetMethod(nameof(BitOperations.LeadingZeroCount), [typeof(uint)]);
        Guard.IsNotNull(clzMethod);
        _clzMethod = clzMethod;

        var handleAccessResultMethod = typeof(MipsJitCompiler<T, TFloat>).GetMethod(nameof(HandleMemoryAccessResult), BindingFlags.Static | BindingFlags.NonPublic);
        Guard.IsNotNull(handleAccessResultMethod);
        _handleAccessResultMethod = handleAccessResultMethod;

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

        InitTables(_cpu.Config);
    }

    /// <summary>
    /// Compiles a block of MIPS code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public MipsJitBlock<T, TFloat> CompileBlock(T startPc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T, TFloat>), typeof(MipsTrap).MakeByRefType()];
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

        var @delegate = (MipsBlockDelegate<T, TFloat>)method.CreateDelegate(typeof(MipsBlockDelegate<T, TFloat>));
        return new(@delegate, endPc - startPc);
    }

    /// <summary>
    /// Compiles a single instruction as a cli dynamic method.
    /// </summary>
    public MipsBlockDelegate<T, TFloat> CompileLoneInstruction(MipsInstruction inst, T pc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T, TFloat>), typeof(MipsTrap).MakeByRefType()];
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

        return (MipsBlockDelegate<T, TFloat>)method.CreateDelegate(typeof(MipsBlockDelegate<T, TFloat>));
    }

    private void CompileInstruction(ILGenerator il, MipsInstruction instruction, T pc)
    {
        var emitter = _instructionTable.Lookup(instruction);
        emitter(il, instruction, pc);
    }

    private void Shift<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TData>(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, (int)inst.ShiftAmount);
            il.Emit(ilOpCode);
        });
    }

    private void ShiftPlus32<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TData>(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, inst.ShiftAmount + 32);
            il.Emit(ilOpCode);
        });
    }

    private void ShiftVar<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
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
        EmitStoreRegister(il, inst.RD, il =>
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
                il.EmitConv<T>(IsSigned<TData>());
        });
    }

    private void CheckedAluR<TData>(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode, bool isSubtraction)
        where TData : unmanaged, INumber<TData>
    {
        Label noOverflow = il.DefineLabel();

        // Get register locals
        // NOTE: This is safe because these registers will not be written to
        LocalBuilder rs = GetRegisterLocal(inst.RS);
        LocalBuilder rt = GetRegisterLocal(inst.RT);

        // Calculate
        EmitLoadRegister<TData>(il, rs);
        EmitLoadRegister<TData>(il, rt);
        il.Emit(ilOpCode);

        // Store result
        LocalBuilder result = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard<TData>(il, rs, rt, result, () =>
        {
            EmitTrapRet(il, MipsTrap.ArithmeticOverflow, pc);
        }, isSubtraction);

        // Safe Path
        EmitStoreRegister(il, inst.RD, il =>
        {
            il.Emit(OpCodes.Ldloc, result);
            if (sizeof(TData) != sizeof(T))
                il.EmitConv<T>(Sign.Signed);
        });
    }

    private void AluI<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, bool signExtend = false)
        where TData : unmanaged, INumber<TData>
    {
        // Fetch the raw immediate from the instruction
        var rawImm = inst.Immediate;
        var extended = signExtend ? TData.CreateTruncating((long)rawImm) : TData.CreateTruncating((ulong)rawImm);

        EmitStoreRegister(il, inst.RT, il =>
        {
            EmitLoadRegister<TData>(il, inst.RS);
            il.EmitLoadConstant(extended);

            il.Emit(ilOpCode);
        });
    }

    private void CheckedAluI<TData>(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        // Get RS register local
        LocalBuilder rs = GetRegisterLocal(inst.RS);

        // Load Immediate into local (Sign-extended)
        il.EmitLoadConstant(TData.CreateTruncating(inst.Immediate));
        LocalBuilder imm = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, imm);

        // Calculate
        EmitLoadRegister<TData>(il, rs);
        EmitLoadRegister<TData>(il, imm);
        il.Emit(ilOpCode);

        // Store result
        LocalBuilder result = il.DeclareLocal(typeof(TData));
        il.Emit(OpCodes.Stloc, result);

        // Overflow Guard
        EmitOverflowGuard<TData>(il, rs, imm, result, () =>
        {
            EmitTrapRet(il, MipsTrap.ArithmeticOverflow, pc);
        });

        // Safe Path
        EmitStoreRegister(il, inst.RT, il =>
        {
            il.Emit(OpCodes.Ldloc, result);
            if (sizeof(TData) != sizeof(T))
            {
                il.EmitConv<T>(Sign.Signed);
            }
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
        EmitStoreRegister(il, MipsGpRegister.High, il =>
        {
            if (c is not 0)
            {
                EmitLoadRegister(il, MipsGpRegister.High);
            }

            il.Emit(OpCodes.Ldloc, localResult);
            il.EmitLoadConstant(shiftAmount);

            if (bigLong)
            {
                il.Emit(OpCodes.Call, _rightShiftMethods[typeof(TLong)]);
                il.Emit(OpCodes.Call, _castDownMethods[typeof(TLong)]);
            }
            else
            {
                il.Emit(OpCodes.Shr_Un);
                il.EmitConv<TData>();
            }

            if (c is not 0)
            {
                il.Emit(c is > 0 ? OpCodes.Add : OpCodes.Sub);
            }

            il.EmitConv<T>(IsSigned<TData>());
        });

        // Store low
        EmitStoreRegister(il, MipsGpRegister.Low, il =>
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
                il.EmitConv<TData>();
            }

            if (c is not 0)
            {
                il.Emit(c is > 0 ? OpCodes.Add : OpCodes.Sub);
            }

            il.EmitConv<T>();
        });
    }

    private void DivR<TData>(ILGenerator il, MipsInstruction inst, bool signed)
        where TData : unmanaged, INumber<TData>
    {
        Label endDiv = il.DefineLabel();

        // Load operands into locals to keep stack predictable
        var rs = GetRegisterLocal(inst.RS);
        var rt = GetRegisterLocal(inst.RT);

        // Guard against Div-By-Zero
        EmitLoadRegister<TData>(il, rt);
        il.EmitLoadConstant(TData.Zero);
        il.Emit(OpCodes.Beq, endDiv);

        // Calculate and store the remainder to High
        EmitStoreRegister(il, MipsGpRegister.High, il =>
        {
            EmitLoadRegister<TData>(il, rs);
            EmitLoadRegister<TData>(il, rt);
            il.Emit(signed ? OpCodes.Rem : OpCodes.Rem_Un);
            if (sizeof(TData) != sizeof(T))
            {
                il.EmitConv<T>(signed ? Sign.Signed : Sign.Unsigned);
            }
        });

        // Calculate and store the quotient to low
        EmitStoreRegister(il, MipsGpRegister.Low, il =>
        {
            EmitLoadRegister<TData>(il, rs);
            EmitLoadRegister<TData>(il, rt);
            il.Emit(signed ? OpCodes.Div : OpCodes.Div_Un);
            if (sizeof(TData) != sizeof(T))
            {
                il.EmitConv<T>(signed ? Sign.Signed : Sign.Unsigned);
            }
        });

        il.MarkLabel(endDiv);
    }

    private void Load<TData>(ILGenerator il, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        var addrVar = EmitLoadEffectiveAddress<TData>(il, inst, pc, MipsTrap.AddressErrorLoad);
        var trapVar = il.DeclareLocal(typeof(MipsTrap));

        // Allocate a local variable to receive the 'out TData value'
        var dataVar = il.DeclareLocal(typeof(TData));

        // Call Memory.TryRead<TData>(ulong, out TData)
        var tryReadMethod = TryReadMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, GetMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);                // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        il.Emit(OpCodes.Ldloca, dataVar);               // Arg 2: out TData value
        il.Emit(OpCodes.Callvirt, tryReadMethod);       // Call TryRead, returns MemoryAccessResult

        // Evaluate access result
        il.EmitLoadBool(false);                             // isWrite = false
        il.Emit(OpCodes.Call, _handleAccessResultMethod);   // Convert to MIPS Trap
        il.Emit(OpCodes.Stloc, trapVar);

        // Branch to success path if no trap occured
        var successLabel = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, trapVar);
        il.Emit(OpCodes.Brfalse, successLabel);

        // Trap path. Return trap
        EmitTrapRet(il, trapVar, pc);

        // Success path! Write back to RT
        il.MarkLabel(successLabel);
        EmitStoreRegister(il, inst.RT, il =>
        {
            // Load the value filled by TryRead
            il.Emit(OpCodes.Ldloc, dataVar);

            // Sign-Extension / Zero-Extension then convert to T
            il.EmitConv<TData>();
            il.EmitConv<T>();
        });
    }

    private void Store<TData>(ILGenerator il, MipsInstruction inst, T pc)
        where TData : unmanaged
    {
        var addrVar = EmitLoadEffectiveAddress<TData>(il, inst, pc, MipsTrap.AddressErrorStore);
        var trapVar = il.DeclareLocal(typeof(MipsTrap));

        // Call Memory.TryWrite<TData>(ulong, TData)
        var tryWriteMethod = TryWriteMethods[typeof(TData)];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, GetMemoryMethod);
        il.Emit(OpCodes.Ldloc, addrVar);            // Arg 1: ulong addr
        il.Emit(OpCodes.Conv_U8);
        EmitLoadRegister(il, inst.RT);              // Arg 2: TData value (Truncate the RT register value)
        il.EmitConv<TData>();
        il.Emit(OpCodes.Callvirt, tryWriteMethod);  // Call TryRead, returns MemoryAccessResult

        // Evaluate access result
        il.EmitLoadBool(true);                              // isWrite = true
        il.Emit(OpCodes.Call, _handleAccessResultMethod);   // Convert to MIPS Trap
        il.Emit(OpCodes.Stloc, trapVar);

        // Branch to success path if no trap occured
        var successLabel = il.DefineLabel();
        il.Emit(OpCodes.Ldloc, trapVar);
        il.Emit(OpCodes.Brfalse, successLabel);

        // Trap path. Return trap
        EmitTrapRet(il, trapVar, pc);

        // Success
        il.MarkLabel(successLabel);
    }

    private void Jump(ILGenerator il, MipsInstruction inst, T pc, bool link = false)
        => Jump(il, pc, link: link, pushAddress: il =>
    {
        il.EmitLoadConstant(T.CreateTruncating(inst.Address));
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
            EmitStoreRegister(il, MipsGpRegister.ReturnAddress, il => il.EmitLoadConstant(returnAddr));
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
            il.EmitLoadConstant(T.Zero);
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
        il.EmitLoadConstant(T.CreateTruncating(inst.Immediate));
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

    private void Move(ILGenerator il, MipsInstruction inst, OpCode invertedBranch)
    {
        Label noMove = il.DefineLabel();

        EmitLoadRegister(il, inst.RT);
        il.Emit(invertedBranch, noMove);

        // DO move
        EmitStoreRegister(il, inst.RD, il => EmitLoadRegister(il, inst.RS));

        // Do NOT move
        il.MarkLabel(noMove);
    }

    private void MoveFromTo(ILGenerator il, MipsGpRegister from, MipsGpRegister to)
    {
        // Can't writeback to $zero.
        // Skip as no-op
        if (to is MipsGpRegister.Zero)
            return;

        EmitStoreRegister(il, to, il => EmitLoadRegister(il, from));
    }

    private void Lui(ILGenerator il, MipsInstruction inst, T pc)
    {
        uint value = (uint)inst.Immediate << 16;

        EmitStoreRegister(il, inst.RT, il => il.EmitLoadConstant(T.CreateTruncating(value)));
    }

    private void ReservedInstruction(ILGenerator il, MipsInstruction inst, T pc) => EmitTrapRet(il, MipsTrap.ReservedInstruction, pc);

    private void MethodUnary<TData>(ILGenerator il, MipsInstruction inst, Action<ILGenerator> method)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TData>(il, inst.RS);

            method(il);

            if (sizeof(TData) != sizeof(T))
                il.EmitConv<T>();
        });
    }

    private static MipsTrap HandleMemoryAccessResult(MemoryAccessResult result, bool isWrite)
    {
        return result switch
        {
            MemoryAccessResult.Success => MipsTrap.None,

            MemoryAccessResult.TranslationFault when isWrite => MipsTrap.TlbMissStore,
            MemoryAccessResult.TranslationFault => MipsTrap.TlbMissLoad,

            MemoryAccessResult.AddressError when isWrite => MipsTrap.AddressErrorStore,
            MemoryAccessResult.AddressError => MipsTrap.AddressErrorLoad,

            MemoryAccessResult.AccessViolation when isWrite => MipsTrap.AddressErrorStore,
            MemoryAccessResult.AccessViolation => MipsTrap.AddressErrorLoad,

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsTrap>(),
        };
    }
}
