// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public unsafe partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private LocalBuilder[] _regLocals = [];

    private void EmitSetupLocalRegisters(ILGenerator il)
    {
        // Setup local registers
        _regLocals = new LocalBuilder[_cpu.RegisterFile.Count];
        for (int i = 1; i < _cpu.RegisterFile.Count; i++)
            _regLocals[i] = il.DeclareLocal(typeof(T));

        // Load read registers
        foreach (var reg in _loadRegs)
        {
            if (reg is MipsGpRegister.Zero)
                continue;

            // Load register from memory into local i
            EmitStoreRegister(il, reg, () =>
            {
                EmitLoadRegisterAddress(il, reg);
                il.EmitLdind<T>();
            });
        }
    }

    private void EmitFlushLocalRegisters(ILGenerator il)
    {
        foreach (var reg in _storeRegs)
        {
            if (reg is MipsGpRegister.Zero)
                continue;

            // Load register local i into memory
            EmitLoadRegisterAddress(il, reg);
            EmitLoadRegister(il, reg);
            il.EmitStind<T>();
        }
    }

    private void EmitDelaySlot(ILGenerator il, T delaySlotPc)
    {
        uint rawInstr = _cpu.Memory.Read<uint>(ulong.CreateTruncating(delaySlotPc));
        MipsInstruction instr = (MipsInstruction)rawInstr;

        // We dispatch to our existing table to emit the IL for this instruction
        // Note: MIPS forbids putting a jump/branch inside a delay slot!
        CompileInstruction(il, instr, delaySlotPc);
    }

    private void EmitLoadRegister(ILGenerator il, MipsGpRegister register)
        => EmitLoadRegister<T>(il, register);

    private void EmitLoadRegister<TData>(ILGenerator il, MipsGpRegister register)
        where TData : unmanaged, INumber<TData>
    {
        if (register is 0)
        {
            // MIPS $zero is always 0. 
            // We push a constant 0 instead of looking at memory.
            il.EmitLoadConstant(TData.Zero);
            return;
        }

        // Load the register's address then retrieve the value at that address
        il.Emit(OpCodes.Ldloc, _regLocals[(int)register]);

        // Convert the value to TData if neccesary
        if (sizeof(T) != sizeof(TData))
            il.EmitConv<TData>();
    }

    private void EmitLoadRegister<TFloat>(ILGenerator il, MipsFloatRegister register)
        where TFloat : unmanaged
    {
        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        il.EmitLdind<TFloat>();
    }

    private void EmitStoreRegister(ILGenerator il, MipsGpRegister register, Action emitEvaluation)
    {
        if (register is 0)
        {
            // $zero cannot be written to.
            // We still emit the value calculation in case it has side effects,
            // then we immediately pop it off the stack.
            emitEvaluation();
            il.Emit(OpCodes.Pop);
            return;
        }

        // Load the register's address, emit the evaluation instructions, and store the value
        emitEvaluation();
        il.Emit(OpCodes.Stloc, _regLocals[(int)register]);
    }

    private void EmitStoreRegister<TFloat>(ILGenerator il, MipsFloatRegister register, Action emitEvaluation)
        where TFloat : unmanaged
    {
        // Load the register's address, emit the evaluation instructions, and store the value
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        il.EmitStind<TFloat>();
    }

    private void EmitLoadRegisterAddress(ILGenerator il, MipsGpRegister register) => EmitLoadRegisterAddress(il, (int)register, _cpu.RegisterFile.Regs);

    private void EmitLoadRegisterAddress(ILGenerator il, MipsFloatRegister register) => EmitLoadRegisterAddress(il, (int)register, _cpu.FloatProcessor.RegisterFile.Regs);

    private static void EmitLoadRegisterAddress(ILGenerator il, int index, T* regs)
    {
        nint regAddress = (nint)regs + (index * sizeof(T));

        if (nint.Size == 8) il.Emit(OpCodes.Ldc_I8, regAddress);
        else if (nint.Size == 4) il.Emit(OpCodes.Ldc_I4, (int)regAddress);
        else throw new PlatformNotSupportedException($"Unsupported pointer size: {nint.Size}");

        il.Emit(OpCodes.Conv_U);
    }

    /// <remarks>
    /// Set <paramref name="accessFailureTrap"/> to <see cref="MipsTrap.None"/> to skip alignment check.
    /// </remarks>
    private LocalBuilder EmitLoadEffectiveAddress<TData>(ILGenerator il, MipsInstruction inst, T pc, MipsTrap accessFailureTrap = MipsTrap.None)
        where TData : unmanaged
    {
        // Calculate Effective Address (rs + offset)
        EmitLoadRegister(il, inst.RS);
        il.Emit(OpCodes.Ldc_I8, (long)inst.Immediate);
        il.Emit(OpCodes.Add);
        var addrVar = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, addrVar);

        // Alignment Check
        int size = sizeof(TData);
        if (accessFailureTrap is not MipsTrap.None && size > 1)
        {
            Label labelAligned = il.DefineLabel();
            il.Emit(OpCodes.Ldloc, addrVar);
            il.Emit(OpCodes.Ldc_I4, size - 1);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Ldc_I8, 0L);
            il.Emit(OpCodes.Beq, labelAligned);

            // Trap: Address Error Load
            EmitTrapArg(il, accessFailureTrap);
            il.EmitLoadConstant(pc);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(labelAligned);
        }

        return addrVar;
    }

    private void EmitRet(ILGenerator il, T pc) => EmitTrapRet(il, MipsTrap.None, pc);

    private void EmitRet(ILGenerator il, Action<ILGenerator> pushAddress)
    {
        EmitFlushLocalRegisters(il);
        EmitTrapArg(il, MipsTrap.None);
        pushAddress(il);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitTrapArg(ILGenerator il, MipsTrap trap)
    {
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldc_I4, (int)trap);
        il.Emit(OpCodes.Stind_I1);
    }

    private void EmitTrapRet(ILGenerator il, MipsTrap trap, T pc)
    {
        EmitFlushLocalRegisters(il);
        EmitTrapArg(il, trap);
        il.EmitLoadConstant(pc);
        il.Emit(OpCodes.Ret);
    }

    private static Sign IsSigned<TData>()
        where TData : unmanaged, INumber<TData>
    {
        if (typeof(TData) == typeof(sbyte) || typeof(TData) == typeof(short) ||
            typeof(TData) == typeof(int) ||typeof(TData) == typeof(long))
            return Sign.Signed;
        else
            return Sign.Unsigned;
    }

    private static void EmitOverflowGuard<TData>(ILGenerator il, T pc, LocalBuilder rs, LocalBuilder rtOrImm, LocalBuilder result, Label noOverflow, bool isSubtraction = false)
        where TData : unmanaged, INumber<TData>
    {
        // Logic: ((rs ^ result) & (rtOrImm ^ result)) < 0  (for Addition)
        // Logic: ((rs ^ result) & (rs ^ rtOrImm)) < 0      (for Subtraction)

        // First term: (rs ^ result)
        il.Emit(OpCodes.Ldloc, rs);
        il.Emit(OpCodes.Ldloc, result);
        il.Emit(OpCodes.Xor);

        // Second term:
        if (isSubtraction)
        {
            // (rs ^ rtOrImm)
            il.Emit(OpCodes.Ldloc, rs);
            il.Emit(OpCodes.Ldloc, rtOrImm);
            il.Emit(OpCodes.Xor);
        }
        else
        {
            // (rtOrImm ^ result)
            il.Emit(OpCodes.Ldloc, rtOrImm);
            il.Emit(OpCodes.Ldloc, result);
            il.Emit(OpCodes.Xor);
        }

        il.Emit(OpCodes.And);

        // Check sign bit
        il.EmitLoadConstant(TData.Zero);

        il.Emit(OpCodes.Bge, noOverflow);

        // Trap Path (ends block)
        EmitTrapArg(il, MipsTrap.ArithmeticOverflow);
        il.EmitLoadConstant(pc);
        il.Emit(OpCodes.Ret);
    }
}
