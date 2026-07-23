// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public unsafe partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private void EmitDelaySlot(ILGenerator il, T delaySlotPc)
    {
        uint rawInstr = _cpu.Memory.Read<uint>(ulong.CreateTruncating(delaySlotPc));
        MipsInstruction instr = (MipsInstruction)rawInstr;

        // We dispatch to our existing table to emit the IL for this instruction
        // Note: MIPS forbids putting a jump/branch inside a delay slot!
        CompileInstruction(il, instr, delaySlotPc);
    }

    /// <inheritdoc/>
    protected override void EmitSetupLocalRegisters(ILGenerator il) => EmitSetupLocalRegisters(il, _cpu.RegisterFile, _loadRegs);

    /// <inheritdoc/>
    protected override void EmitFlushLocalRegisters(ILGenerator il) => EmitFlushLocalRegisters(il, _cpu.RegisterFile, _storeRegs);

    /// <inheritdoc/>
    protected override void EmitLoadRegister<TData>(ILGenerator il, MipsGpRegister register)
    {
        if (register is 0)
        {
            // MIPS $zero is always 0. 
            // We push a constant 0 instead of looking at memory.
            il.EmitLoadConstant(TData.Zero);
            return;
        }

        base.EmitLoadRegister<TData>(il, register);
    }

    private void EmitLoadRegister<TFloat>(ILGenerator il, MipsFloatRegister register)
        where TFloat : unmanaged
    {
        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        il.EmitLdind<TFloat>();
    }

    /// <inheritdoc/>
    protected override void EmitStoreRegister(ILGenerator il, MipsGpRegister register, Action<ILGenerator> emitEvaluation)
    {
        if (register is 0)
        {
            // $zero cannot be written to.
            // We still emit the value calculation in case it has side effects,
            // then we immediately pop it off the stack.
            emitEvaluation(il);
            il.Emit(OpCodes.Pop);
            return;
        }

        base.EmitStoreRegister(il, register, emitEvaluation);
    }

    private void EmitStoreRegister<TFloat>(ILGenerator il, MipsFloatRegister register, Action emitEvaluation)
        where TFloat : unmanaged
    {
        // Load the register's address, emit the evaluation instructions, and store the value
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        il.EmitStind<TFloat>();
    }

    private void EmitLoadRegisterAddress(ILGenerator il, MipsFloatRegister register) => EmitLoadRegisterAddress(il, (int)register, _cpu.FloatProcessor.RegisterFile.Regs);

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
            EmitTrapRet(il, accessFailureTrap, pc);

            il.MarkLabel(labelAligned);
        }

        return addrVar;
    }

    private void EmitRet(ILGenerator il, T pc) => EmitTrapRet(il, MipsTrap.None, pc);

    private void EmitRet(ILGenerator il, Action<ILGenerator> pushAddress)
    {
        EmitFlushLocalRegisters(il, _cpu.RegisterFile, _storeRegs);
        EmitTrapArg(il, MipsTrap.None);
        pushAddress(il);
        il.Emit(OpCodes.Ret);
    }
}
