// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Machine.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.JIT;

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

    private void EmitLoadRegisterAddress(ILGenerator il, MipsGpRegister register)
    {
#if DEBUG
        // This method should not be used for the $zero register.
        // For reads a constant 0 should be loaded, and for writes the value
        // should be discarded.
        Guard.IsNotEqualTo((int)register, (int)MipsGpRegister.Zero);
#endif

        nint baseAddress = (nint)_cpu.RegisterFile.Regs;
        nint regAddress = baseAddress + ((int)register * sizeof(T));

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

    private void EmitTrapArg(ILGenerator il, MipsTrap trap)
    {
        il.Emit(OpCodes.Ldarg, 1);
        il.Emit(OpCodes.Ldc_I4, (int)trap);
        il.Emit(OpCodes.Stind_I4);
    }

    private static void EmitLdind(ILGenerator il)
    {
        if (typeof(T) == typeof(uint))
        {
            il.Emit(OpCodes.Ldind_U4);
        }
        else if (typeof(T) == typeof(ulong))
        {
            il.Emit(OpCodes.Ldind_I8);
        }
        else
        {
            throw new NotSupportedException("Unsupported register width.");
        }
    }

    private static void EmitStind(ILGenerator il)
    {
        if (typeof(T) == typeof(uint))
        {
            il.Emit(OpCodes.Stind_I4);
        }
        else if (typeof(T) == typeof(ulong))
        {
            il.Emit(OpCodes.Stind_I8);
        }
        else
        {
            throw new NotSupportedException("Unsupported register width.");
        }
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

    private void EmitOverflowGuard(ILGenerator il, T pc, bool isSubtraction, LocalBuilder rs, LocalBuilder rtOrImm, LocalBuilder result, Label noOverflow)
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
