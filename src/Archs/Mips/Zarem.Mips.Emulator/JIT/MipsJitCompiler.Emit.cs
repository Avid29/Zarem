// Avishai Dernis 2026

using System;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public unsafe partial class MipsJitCompiler<T, TFloat>
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

    private void EmitLoadRegister<TFloatData>(ILGenerator il, MipsFloatRegister register)
        where TFloatData : unmanaged
    {
        // In legacy mode, double precision floating point values are stored in two consecutive single precision registers.
        // We use paired registers when we are loading a double precision value (TFloatData is 64 bits) and we are in legacy mode.
        bool legacyMode = sizeof(TFloat) == sizeof(uint) || !_cpu.CoProcessor0.FloatingPoint64BitMode;
        bool usePairedRegisters = sizeof(TFloatData) == sizeof(ulong) && legacyMode;

        if (usePairedRegisters)
        {
            // Load the upper half of the double (the second 32 bits)
            EmitLoadRegisterAddress(il, register + 1);
            il.EmitLdind<TFloat>();
            il.EmitConv<uint>();
            il.EmitConv<ulong>();
            il.EmitLoadConstant(32);                        // Shift the upper half left by 32 bits to make room for the lower half
            il.Emit(OpCodes.Shl);
            EmitLoadRegisterAddress(il, register);          // Load the lower half of the double (the first 32 bits)
            il.EmitLdind<TFloat>();
            il.EmitConv<uint>();
            il.EmitConv<ulong>();
            il.Emit(OpCodes.Or);                            // Combine the two halves into a single 64-bit value

            // If the target type is not ulong, we need to convert the 64-bit value to the appropriate type
            if (typeof(TFloatData) != typeof(ulong))
            {
                LocalBuilder bitStorage = il.DeclareLocal(typeof(ulong));
                il.Emit(OpCodes.Stloc, bitStorage);
                il.Emit(OpCodes.Ldloca, bitStorage);
                il.EmitLdind<TFloatData>();
            }
        }
        else
        {
            // Load the register's address then retrieve the value at that address
            EmitLoadRegisterAddress(il, register);
            il.EmitLdind<TFloatData>();
        }
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

    private void EmitStoreRegister<TFloatData>(ILGenerator il, MipsFloatRegister register, Action emitEvaluation)
        where TFloatData : unmanaged
    {
        // In legacy mode, double precision floating point values are stored in two consecutive single precision registers.
        // We use paired registers when we are loading a double precision value (TFloatData is 64 bits) and we are in legacy mode.
        bool legacyMode = sizeof(TFloat) == sizeof(uint) || !_cpu.CoProcessor0.FloatingPoint64BitMode;
        bool usePairedRegisters = sizeof(TFloatData) == sizeof(ulong) && legacyMode;

        if (usePairedRegisters)
        {
            // 1. Evaluate the incoming expression and store it as a 64-bit bitmask (ulong)
            LocalBuilder value64Local = il.DeclareLocal(typeof(ulong));
            emitEvaluation();

            // If the target type is not ulong, we need to convert the 64-bit value to the appropriate type
            if (typeof(TFloatData) == typeof(double))
            {
                LocalBuilder doubleLocal = il.DeclareLocal(typeof(double));
                il.Emit(OpCodes.Stloc, doubleLocal);
                il.Emit(OpCodes.Ldloca, doubleLocal);
                il.EmitLdind<ulong>();
            }
            il.Emit(OpCodes.Stloc, value64Local);

            // Store Lower 32 Bits into _regs[register]
            EmitLoadRegisterAddress(il, register);

            // For host-endian safety: Load existing full TFloat native slot first
            il.Emit(OpCodes.Dup);
            il.EmitLdind<TFloat>();
            il.Emit(OpCodes.Conv_U8);
            il.EmitLoadConstant(0xFFFFFFFF00000000UL);      // Mask out the lower 32 bits of the existing register value
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldloc, value64Local);           // Isolate our new lower 32 bits from our evaluated local
            il.Emit(OpCodes.Conv_U4);
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Or);                            // Merge and write back out to the full native memory slot
            il.EmitConv<TFloat>();
            il.EmitStind<TFloat>();

            // Store Upper 32 Bits into _regs[register + 1]
            EmitLoadRegisterAddress(il, register + 1);

            // Load existing full TFloat native slot
            il.Emit(OpCodes.Dup);
            il.EmitLdind<TFloat>();
            il.Emit(OpCodes.Conv_U8);
            il.EmitLoadConstant(0xFFFFFFFF00000000UL);      // Mask out the lower 32 bits of the existing register value
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Ldloc, value64Local);           // Isolate our new upper 32 bits by shifting right logically
            il.EmitLoadConstant(32);
            il.Emit(OpCodes.Shr_Un);
            il.Emit(OpCodes.Conv_U4);
            il.Emit(OpCodes.Conv_U8);
            il.Emit(OpCodes.Or);                            // Merge and write back out to the full native memory slot
            il.EmitConv<TFloat>();
            il.EmitStind<TFloat>();
        }
        else
        {
            // Load the register's address, emit the evaluation instructions, and store the value
            EmitLoadRegisterAddress(il, register);
            emitEvaluation();
            il.EmitStind<TFloatData>();
        }
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
