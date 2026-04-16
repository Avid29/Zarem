// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Machine.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

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

    private void EmitLoadRegister(ILGenerator il, MipsGpRegister register)
    {
        if (register is 0)
        {
            // MIPS $zero is always 0. 
            // We push a constant 0 instead of looking at memory.
            EmitLoadConstant(il, T.Zero);
            return;
        }

        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        EmitLdind(il);
    }

    private void EmitLoadRegister<TFloat>(ILGenerator il, MipsFloatRegister register)
        where TFloat : unmanaged
    {
        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        EmitLdind<TFloat>(il);
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
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        EmitStind(il);
    }

    private void EmitStoreRegister<TFloat>(ILGenerator il, MipsFloatRegister register, Action emitEvaluation)
        where TFloat : unmanaged
    {
        // Load the register's address, emit the evaluation instructions, and store the value
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        EmitStind<TFloat>(il);
    }

    private void EmitLoadRegisterAddress(ILGenerator il, MipsGpRegister register) => EmitLoadRegisterAddress(il, (int)register, _cpu.RegisterFile.Regs);

    private void EmitLoadRegisterAddress(ILGenerator il, MipsFloatRegister register) => EmitLoadRegisterAddress(il, (int)register, _cpu.FloatProcessor.RegisterFile.Regs);

    private static void EmitLoadRegisterAddress(ILGenerator il, int index, T* regs)
    {
        nint regAddress = (nint)regs + (index * sizeof(T));

        if (IntPtr.Size == 4)
        {
            il.Emit(OpCodes.Ldc_I4, regAddress);
        }
        else
        {
            il.Emit(OpCodes.Ldc_I8, regAddress);
        }

        il.Emit(OpCodes.Conv_I);
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
            EmitLoadConstant(il, pc);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(labelAligned);
        }

        return addrVar;
    }

    private static void EmitTrapArg(ILGenerator il, MipsTrap trap)
    {
        il.Emit(OpCodes.Ldarg, 1);
        il.Emit(OpCodes.Ldc_I4, (int)trap);
        il.Emit(OpCodes.Stind_I4);
    }

    private static void EmitLdind(ILGenerator il) => EmitLdind<T>(il);

    private static void EmitLdind<TData>(ILGenerator il)
    {
        if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Ldind_I4);
        else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Ldind_I4);
        else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Ldind_R4);
        else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Ldind_I8);
        else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Ldind_I8);
        else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Ldind_R8);
        else throw new NotSupportedException("Unsupported register width.");
    }

    private static void EmitStind(ILGenerator il) => EmitStind<T>(il);

    private static void EmitStind<TData>(ILGenerator il)
        where TData : unmanaged
    {
        if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Stind_I4);
        else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Stind_I4);
        else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Stind_R4);
        else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Stind_I8);
        else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Stind_I8);
        else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Stind_R8);
        else throw new NotSupportedException("Unsupported register width.");
    }

    private static void EmitConv(ILGenerator il) => EmitConv<T>(il);

    private static void EmitConv<TData>(ILGenerator il)
    {
        if (typeof(TData) == typeof(sbyte)) il.Emit(OpCodes.Conv_I1);
        else if (typeof(TData) == typeof(byte)) il.Emit(OpCodes.Conv_U1);
        else if (typeof(TData) == typeof(short)) il.Emit(OpCodes.Conv_I2);
        else if (typeof(TData) == typeof(ushort)) il.Emit(OpCodes.Conv_U2);
        else if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Conv_I4);
        else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Conv_U4);
        else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Conv_R4);
        else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Conv_I8);
        else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Conv_U8);
        else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Conv_R8);
    }

    private static void EmitLoadConstant(ILGenerator il, T value)
    {
        if (typeof(T) == typeof(uint))
        {
            il.Emit(OpCodes.Ldc_I4, uint.CreateTruncating(value));
        }
        else if (typeof(T) == typeof(ulong))
        {
            il.Emit(OpCodes.Ldc_I8, ulong.CreateTruncating(value));
        }
        else
        {
            throw new NotSupportedException("Unsupported register width.");
        }
    }

    private static void EmitOverflowGuard(ILGenerator il, T pc, bool isSubtraction, LocalBuilder rs, LocalBuilder rtOrImm, LocalBuilder result, Label noOverflow)
    {
        // Logic: ((rs ^ result) & (rtOrImm ^ result)) < 0  (for Addition)
        // Logic: ((rs ^ result) & (rs ^ rtOrImm)) < 0     (for Subtraction)

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
        if (sizeof(T) == 4) il.Emit(OpCodes.Ldc_I4_0);
        else il.Emit(OpCodes.Ldc_I8, 0L);

        il.Emit(OpCodes.Bge, noOverflow);

        // Trap Path (ends block)
        EmitTrapArg(il, MipsTrap.ArithmeticOverflow);
        EmitLoadConstant(il, pc);
        il.Emit(OpCodes.Ret);
    }
}
