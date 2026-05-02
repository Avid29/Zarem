// Avishai Dernis 2026

using System;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T>
{
    /// <inheritdoc/>
    protected override void EmitSetupLocalRegisters(ILGenerator il) => EmitSetupLocalRegisters(il, _cpu.RegisterFile, _loadRegs);

    /// <inheritdoc/>
    protected override void EmitFlushLocalRegisters(ILGenerator il) => EmitFlushLocalRegisters(il, _cpu.RegisterFile, _storeRegs);

    /// <inheritdoc/>
    protected override void EmitLoadRegister<TData>(ILGenerator il, RiscVGpRegister register)
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

    /// <inheritdoc/>
    protected override void EmitStoreRegister(ILGenerator il, RiscVGpRegister register, Action<ILGenerator> emitEvaluation)
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

    /// <remarks>
    /// Set <paramref name="accessFailureTrap"/> to <see cref="RiscVTrap.None"/> to skip alignment check.
    /// </remarks>
    private LocalBuilder EmitLoadEffectiveAddress<TData>(ILGenerator il, RiscVInstruction inst, T pc, short immediate, RiscVTrap accessFailureTrap = RiscVTrap.None)
        where TData : unmanaged
    {
        // Calculate Effective Address (rs1 + offset)
        EmitLoadRegister(il, inst.RS1);
        il.Emit(OpCodes.Ldc_I8, (long)immediate);
        il.Emit(OpCodes.Add);
        var addrVar = il.DeclareLocal(typeof(T));
        il.Emit(OpCodes.Stloc, addrVar);

        // Alignment Check
        int size = sizeof(TData);
        if (accessFailureTrap is not RiscVTrap.None && size > 1)
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


    private void EmitRet(ILGenerator il, T pc) => EmitTrapRet(il, RiscVTrap.None, pc);

    private void EmitRet(ILGenerator il, Action<ILGenerator> pushAddress)
    {
        EmitFlushLocalRegisters(il, _cpu.RegisterFile, _storeRegs);
        EmitTrapArg(il, RiscVTrap.None);
        pushAddress(il);
        il.Emit(OpCodes.Ret);
    }
}
