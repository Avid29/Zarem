// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public unsafe partial class MipsJitCompiler<T>
{
    private void EmitDelaySlot(ILGenerator il, T delaySlotPc)
    {
        uint rawInstr = Cpu.Memory.Read<uint>(ulong.CreateTruncating(delaySlotPc));
        MipsInstruction instr = (MipsInstruction)rawInstr;

        // We dispatch to our existing table to emit the IL for this instruction
        // Note: MIPS forbids putting a jump/branch inside a delay slot!
        CompileInstruction(il, instr, delaySlotPc);
    }

    /// <inheritdoc/>
    protected override void EmitLoadRegister<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsGpRegister register)
    {
        if (register is 0)
        {
            // MIPS $zero is always 0. 
            // We push a constant 0 instead of looking at memory.
            context.ILGenerator.EmitLoadConstant(TData.Zero);
            return;
        }

        base.EmitLoadRegister<TData>(context, register);
    }

    /// <inheritdoc/>
    protected override void EmitStoreRegister(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsGpRegister register, Action<ILGenerator> emitEvaluation)
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
    /// Set <paramref name="accessFailureTrap"/> to <see cref="MipsTrap.None"/> to skip alignment check.
    /// </remarks>
    private LocalBuilder EmitLoadEffectiveAddress<TData>(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst, T pc, MipsTrap accessFailureTrap = MipsTrap.None)
        where TData : unmanaged
    {
        var il = context.ILGenerator;

        // Calculate Effective Address (rs + offset)
        EmitLoadRegister(context, inst.RS);
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
            EmitTrapRet(context, accessFailureTrap, pc);

            il.MarkLabel(labelAligned);
        }

        return addrVar;
    }

    private void EmitRet(JitBlockContext<T, MipsGpRegister, MipsTrap> context, T pc) => EmitTrapRet(context, MipsTrap.None, pc);

    private void EmitRet(JitBlockContext<T, MipsGpRegister, MipsTrap> context, Action<ILGenerator> pushAddress)
    {
        var il = context.ILGenerator;
        context.EmitFlushLocalRegisters(this);
        EmitTrapArg(il, MipsTrap.None);
        pushAddress(il);
        il.Emit(OpCodes.Ret);
    }
}
