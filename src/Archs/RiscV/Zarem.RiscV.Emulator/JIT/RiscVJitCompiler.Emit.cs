// Avishai Dernis 2026

using System;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T, TFloat>
{
    /// <inheritdoc/>
    public override void EmitLoadRegister<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVGpRegister register)
    {
        var il = context.ILGenerator;

        if (register is 0)
        {
            // RISC-V $zero is always 0. 
            // We push a constant 0 instead of looking at memory.
            il.EmitLoadConstant(TData.Zero);
            return;
        }

        base.EmitLoadRegister<TData>(context, register);
    }

    /// <inheritdoc/>
    public override void EmitStoreRegister(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVGpRegister register, Action<ILGenerator> emitEvaluation)
    {
        var il = context.ILGenerator;

        if (register is 0)
        {
            // $zero cannot be written to.
            // We still emit the value calculation in case it has side effects,
            // then we immediately pop it off the stack.
            emitEvaluation(il);
            il.Emit(OpCodes.Pop);
            return;
        }

        base.EmitStoreRegister(context, register, emitEvaluation);
    }

    /// <remarks>
    /// Set <paramref name="accessFailureTrap"/> to <see cref="RiscVTrap.None"/> to skip alignment check.
    /// </remarks>
    private LocalBuilder EmitLoadEffectiveAddress<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, short immediate, RiscVTrap accessFailureTrap = RiscVTrap.None)
        where TData : unmanaged
    {
        var il = context.ILGenerator;

        // Calculate Effective Address (rs1 + offset)
        EmitLoadRegister(context, inst.RS1);
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
            EmitTrapRet(context, accessFailureTrap, pc);

            il.MarkLabel(labelAligned);
        }

        return addrVar;
    }

    private void EmitRet(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, T pc) => EmitTrapRet(context, RiscVTrap.None, pc);

    private void EmitRet(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, Action<ILGenerator> pushAddress)
    {
        var il = context.ILGenerator;
        context.EmitFlushLocalRegisters(this);
        EmitTrapArg(il, RiscVTrap.None);
        pushAddress(il);
        il.Emit(OpCodes.Ret);
    }
}
