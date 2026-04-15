// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
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

    private void EmitRegisterRead(ILGenerator il, MipsGpRegister register)
    {
        if (register is 0)
        {
            // MIPS $zero is always 0. 
            // We push a constant 0 instead of looking at memory.
            EmitLoadConstant(il, T.Zero);
            return;
        }

        long baseAddress = (long)_cpu.RegisterFile.Regs;
        long regAddress = baseAddress + ((int)register * sizeof(T));

        if (IntPtr.Size == 4)
        {
            il.Emit(OpCodes.Ldc_I4, (int)regAddress);
        }
        else
        {
            il.Emit(OpCodes.Ldc_I8, regAddress);
        }

        il.Emit(OpCodes.Conv_I);
        EmitLdind(il);
    }

    private void EmitRegisterWrite(ILGenerator il, MipsGpRegister register, Action emitValue)
    {
        if (register is 0)
        {
            // $zero cannot be written to.
            // We still emit the value calculation in case it has side effects,
            // then we immediately pop it off the stack.
            emitValue();
            il.Emit(OpCodes.Pop);
            return;
        }

        // Calculate and push the static address of the register
        long baseAddress = (long)_cpu.RegisterFile.Regs;
        long regAddress = baseAddress + ((int)register * sizeof(T));

        if (IntPtr.Size == 4)
        {
            il.Emit(OpCodes.Ldc_I4, (int)regAddress);
        }
        else
        {
            il.Emit(OpCodes.Ldc_I8, regAddress);
        }

        il.Emit(OpCodes.Conv_I);

        // Emit the logic to calculate the value (pushes result to stack)
        emitValue();

        // Store the value into the address
        // Stind expects [address, value] on the stack
        EmitStind(il);
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
}
