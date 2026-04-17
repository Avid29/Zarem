// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Machine.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public unsafe partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private delegate bool MipsEmitter(ILGenerator il, MipsInstruction inst, T pc);

    private readonly MipsEmitter[] _opCodeTable = new MipsEmitter[64];
    private readonly MipsEmitter[] _specialTable = new MipsEmitter[64];
    private readonly MipsEmitter[] _special2Table = new MipsEmitter[64];
    private readonly MipsEmitter[] _regImmTable = new MipsEmitter[32];
    private readonly MipsJitCpu<T> _cpu;

    private readonly MethodInfo _getMemoryMethod;
    private readonly Dictionary<Type, MethodInfo> _readMethods = [];
    private readonly Dictionary<Type, MethodInfo> _writeMethods = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitCompiler{T}"/> class.
    /// </summary>
    public MipsJitCompiler(MipsJitCpu<T> cpu)
    {
        _cpu = cpu;

        var getMemoryMethod = _cpu.GetType().GetProperty("Memory")?.GetGetMethod();
        Guard.IsNotNull(getMemoryMethod);
        _getMemoryMethod = getMemoryMethod;

        Type[] readWritetypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint)];
        foreach (var type in readWritetypes)
        {
            _readMethods[type] = _cpu.Memory.GetType()
                .GetMethods()
                .First(m => m.Name == "Read" && m.IsGenericMethod && m.GetParameters().Length == 1)
                .MakeGenericMethod(type);
            _writeMethods[type] = _cpu.Memory.GetType()
                .GetMethods()
                .First(m => m.Name == "Write" && m.IsGenericMethod && m.GetParameters().Length == 2)
                .MakeGenericMethod(type);
        }


        InitTables(_cpu.Config);
    }

    /// <summary>
    /// Compiles a block of MIPS code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public MipsBlockDelegate<T> CompileBlock(T startPc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T>), typeof(MipsTrap).MakeByRefType()];
        var method = new DynamicMethod($"Block_0x{startPc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();

        T currentPc = startPc;
        bool isFinished = false;

        while (!isFinished)
        {
            var inst = (MipsInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(currentPc));
            isFinished = CompileInstruction(il, inst, currentPc);
            currentPc += T.CreateTruncating(4);
        }

        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }

    /// <summary>
    /// Compiles a single instruction as a cli dynamic method.
    /// </summary>
    public MipsBlockDelegate<T> CompileLoneInstruction(MipsInstruction inst, T pc)
    {
        Type[] parameterTypes = [typeof(MipsJitCpu<T>), typeof(MipsTrap).MakeByRefType()];
        var method = new DynamicMethod(
            $"Insert_0x{pc:X}",
            typeof(T),
            parameterTypes,
            this.GetType(),
            true);

        var il = method.GetILGenerator();
        bool ended = CompileInstruction(il, inst, pc);

        if (!ended)
        {
            EmitRet(il, pc + T.CreateTruncating(4));
        }

        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }

    private bool CompileInstruction(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emitter = _opCodeTable[(int)inst.OpCode];
        return emitter(il, inst, pc);
    }

    private bool DispatchSpecial(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emmiter = _specialTable[(int)inst.FuncCode];
        return emmiter(il, inst, pc);
    }

    private bool DispatchRegImm(ILGenerator il, MipsInstruction inst, T pc)
    {
        var emmiter = _regImmTable[(int)inst.RTFuncCode];
        return emmiter(il, inst, pc);
    }

    private bool Shift<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister<TData>(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, (int)inst.ShiftAmount);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool ShiftPlus32<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, () =>
        {
            EmitLoadRegister<TData>(il, inst.RT);
            il.Emit(OpCodes.Ldc_I4, inst.ShiftAmount + 32);
            il.Emit(ilOpCode);
        });

        return false;
    }

    private bool ShiftVar<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode)
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
        return false;
    }

    private bool AluR<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, OpCode? followUp = null)
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
            if (typeof(TData) != typeof(T))
                EmitConv(il);
        });

        return false;
    }

    private bool CheckedAluR<TData>(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode, bool isSubtraction)
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
            if (typeof(TData) != typeof(T))
                EmitConv(il);
        });

        return false;
    }

    private bool AluI<TData>(ILGenerator il, MipsInstruction inst, OpCode ilOpCode, bool signExtend = false)
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

        return false;
    }

    private bool CheckedAluI<TData>(ILGenerator il, MipsInstruction inst, T pc, OpCode ilOpCode)
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
            if (typeof(TData) != typeof(T))
                EmitConv(il);
        });

        return false;
    }

    private bool MultR(ILGenerator il, MipsInstruction inst, bool signed)
    {
        // Retrieve the rs/rt registers
        EmitLoadRegister(il, inst.RS);
        il.Emit(signed ? OpCodes.Conv_I8 : OpCodes.Conv_U8);
        EmitLoadRegister(il, inst.RT);
        il.Emit(signed ? OpCodes.Conv_I8 : OpCodes.Conv_U8);

        // Apply multiplication and store result as a local
        var localResult = il.DeclareLocal(typeof(long));
        il.Emit(OpCodes.Mul);
        il.Emit(OpCodes.Stloc, localResult);

        // Store high
        EmitLoadRegisterAddress(il, MipsGpRegister.High);
        il.Emit(OpCodes.Ldloc, localResult);
        il.Emit(OpCodes.Ldc_I4, 32);
        il.Emit(OpCodes.Shr_Un);
        il.Emit(OpCodes.Conv_U4);
        il.Emit(OpCodes.Stind_I4);

        // Store low
        EmitLoadRegisterAddress(il, MipsGpRegister.Low);
        il.Emit(OpCodes.Ldloc, localResult);
        il.Emit(OpCodes.Conv_U4);
        il.Emit(OpCodes.Stind_I4);

        return false;
    }

    private bool DivR(ILGenerator il, MipsInstruction inst, bool signed)
    {
        Label endDiv = il.DefineLabel();

        // Load operands into locals to keep stack predictable
        EmitLoadRegister(il, inst.RS);
        LocalBuilder rsLocal = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rsLocal);
        EmitLoadRegister(il, inst.RT);
        LocalBuilder rtLocal = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, rtLocal);

        // Guard against Div-By-Zero
        il.Emit(OpCodes.Ldloc, rtLocal);
        il.Emit(OpCodes.Ldc_I4_0);
        if (sizeof(T) == 8)
        {
            il.Emit(OpCodes.Conv_I8); // Ensure width matches T
        }
        il.Emit(OpCodes.Beq, endDiv);

        // Calculate and store the remainder to High
        EmitLoadRegisterAddress(il, MipsGpRegister.High);
        il.Emit(OpCodes.Ldloc, rsLocal);
        il.Emit(OpCodes.Ldloc, rtLocal);
        il.Emit(signed ? OpCodes.Rem : OpCodes.Rem_Un);
        if (sizeof(T) == 4)
        {
            il.Emit(OpCodes.Conv_U4);
        }
        EmitStind(il);

        // Calculate and store the quotient to low
        EmitLoadRegisterAddress(il, MipsGpRegister.Low);
        il.Emit(OpCodes.Ldloc, rsLocal);
        il.Emit(OpCodes.Ldloc, rtLocal);
        il.Emit(signed ? OpCodes.Div : OpCodes.Div_Un);
        if (sizeof(T) == 4)
        {
            il.Emit(OpCodes.Conv_U4);
        }
        EmitStind(il);

        il.MarkLabel(endDiv);

        return false;
    }
    
    private bool Load<TData>(ILGenerator il, MipsInstruction inst, T pc)
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

        return false;
    }

    private bool Store<TData>(ILGenerator il, MipsInstruction inst, T pc)
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

        return false; // Does not complete the block
    }

    private bool Jump(ILGenerator il, MipsInstruction inst, T pc, bool link = false) => Jump(il, inst, pc, link: link, pushAddress: il =>
    {
        EmitLoadConstant(il, T.CreateTruncating(inst.Address));
    });

    private bool JumpR(ILGenerator il, MipsInstruction inst, T pc, bool link = false) => Jump(il, inst, pc, link: link, pushAddress: il =>
    {
        EmitLoadRegister(il, inst.RS);
    });

    private bool Jump(ILGenerator il, MipsInstruction inst, T pc, Action<ILGenerator> pushAddress, bool link = false)
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

        return true; // Signals the compiler that this block is finished
    }

    private bool TrapCompareReg(ILGenerator il, MipsInstruction inst, T pc, OpCode invertedBranch) => ConditionalTrap(il, inst, pc, invertedBranch, il =>
    {
        EmitLoadRegister(il, inst.RS);
        EmitLoadRegister(il, inst.RT);
    });

    private bool TrapCompareImmediate(ILGenerator il, MipsInstruction inst, T pc, OpCode invertedBranch) => ConditionalTrap(il, inst, pc, invertedBranch, il =>
    {
        EmitLoadRegister(il, inst.RS);
        EmitLoadConstant(il, T.CreateTruncating(inst.Immediate));
    });

    private static bool ConditionalTrap(ILGenerator il, MipsInstruction inst, T pc, OpCode invertedBranch, Action<ILGenerator> pushOperands)
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

        return true;
    }

    private static bool Trap(ILGenerator il, T pc, MipsTrap trap)
    {
        EmitTrapRet(il, trap, pc);
        return true; // Terminate the IL block here
    }

    private bool BranchCompareReg(ILGenerator il, MipsInstruction inst, T pc, OpCode conditionOpCode) => Branch(il, inst, pc, conditionOpCode, il =>
    {
        EmitLoadRegister(il, inst.RS);
        EmitLoadRegister(il, inst.RT);
    });

    private bool BranchCompareZero(ILGenerator il, MipsInstruction inst, T pc, OpCode conditionOpCode) => Branch(il, inst, pc, conditionOpCode, il =>
    {
        EmitLoadRegister(il, inst.RS);
        EmitLoadConstant(il, T.Zero);
    });

    private bool Branch(ILGenerator il, MipsInstruction inst, T pc, OpCode conditionOpCode, Action<ILGenerator> pushOperands, bool likely = false)
    {
        bool delaySlots = !_cpu.Config.DisableDelaySlots;

        Label takeBranch = il.DefineLabel();

        // Prepare the stack for the branch condition
        pushOperands(il);

        // Append delay slot operation
        if (!delaySlots && !likely)
        {
            EmitDelaySlot(il, pc + T.CreateTruncating(4));
        }

        // Evaluate the branch condition
        il.Emit(conditionOpCode, takeBranch);

        // Branch NOT taken
        EmitRet(il, pc + (delaySlots ? T.CreateTruncating(8) : T.CreateTruncating(4)));

        // Branch taken
        il.MarkLabel(takeBranch);
        long offset = (long)inst.Immediate << 2;
        T targetPc = pc + T.CreateTruncating(4) + T.CreateTruncating(offset);
        EmitRet(il, targetPc);
        return true;
    }

    private bool MoveFromTo(ILGenerator il, MipsGpRegister from, MipsGpRegister to)
    {
        // Can't writeback to $zero.
        // Skip as no-op
        if (to is MipsGpRegister.Zero)
            return false;

        EmitStoreRegister(il, to, () =>
        {
            EmitLoadRegister(il, from);
        });

        return false;
    }

    private bool Lui(ILGenerator il, MipsInstruction inst)
    {
        uint value = (uint)inst.Immediate << 16;

        EmitStoreRegister(il, inst.RT, () =>
        {
            EmitLoadConstant(il, T.CreateTruncating(value));
        });

        return false;
    }
}
