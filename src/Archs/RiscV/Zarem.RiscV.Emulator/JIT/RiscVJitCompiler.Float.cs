// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T, TFloat>
{
    private void FloatAlu<TFormat>(ILGenerator il, RiscVFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(il, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            EmitLoadRegister<TFormat>(il, inst.FRS2);
            il.Emit(ilOpCode);
        });
    }

    private void FloatUnary<TFormat>(ILGenerator il, RiscVFloatInstruction inst, T pc, string methodName)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        EmitStoreRegister<TFormat>(il, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            var method = typeof(TFormat).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Guard.IsNotNull(method);
            il.Emit(OpCodes.Call, method);
        });
    }

    private void FloatCompare<TFormat>(ILGenerator il, RiscVFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister(il, ((RiscVInstruction)inst).RD, _ =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            EmitLoadRegister<TFormat>(il, inst.FRS2);
            il.Emit(ilOpCode);
        });
    }

    private void FloatFle<TFormat>(ILGenerator il, RiscVFloatInstruction inst)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        // No direct IL opcode for floating-point less than or equal,
        // so we use a combination of comparison and logical operations

        EmitStoreRegister(il, ((RiscVInstruction)inst).RD, _ =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            EmitLoadRegister<TFormat>(il, inst.FRS2);

            // Compare great than unsigned,
            // then negate the result to get less than or equal
            il.Emit(OpCodes.Cgt_Un);
            il.EmitLoadConstant(0);
            il.Emit(OpCodes.Ceq);
        });
    }

    private void FloatMinMax<TFormat>(ILGenerator il, RiscVFloatInstruction inst, T pc)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        // If the instruction is not a float min or max, make an illegal instruction trap
        if (inst.Funct3 is not (FloatFunct3Code.FloatMin or FloatFunct3Code.FloatMax))
            IllegalInstruction(il, inst, pc);

        EmitStoreRegister<TFormat>(il, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            EmitLoadRegister<TFormat>(il, inst.FRS2);
            var method = inst.Funct3 switch
            {
                FloatFunct3Code.FloatMin => typeof(TFormat).GetMethod(nameof(TFormat.Min), BindingFlags.Public | BindingFlags.Static),
                FloatFunct3Code.FloatMax => typeof(TFormat).GetMethod(nameof(TFormat.Max), BindingFlags.Public | BindingFlags.Static),
                _ => throw new NotSupportedException($"Unsupported float function: {inst.Funct3}"),
            };

            Guard.IsNotNull(method);
            il.Emit(OpCodes.Call, method);
        });
    }

    private void EmitLoadRegister<TFloatData>(ILGenerator il, RiscVFloatRegister register)
        where TFloatData : unmanaged
    {
        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        il.EmitLdind<TFloatData>();
    }

    private void EmitStoreRegister<TFloatData>(ILGenerator il, RiscVFloatRegister register, Action emitEvaluation)
        where TFloatData : unmanaged
    {
        // Load the register's address, emit the evaluation instructions, and store the value
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        il.EmitStind<TFloatData>();
    }
    
    private void EmitLoadRegisterAddress(ILGenerator il, RiscVFloatRegister register)
    {
        Guard.IsNotNull(_cpu.FloatRegisterFile);

        EmitLoadRegisterAddress(il, (int)register, _cpu.FloatRegisterFile.Regs);
    }
}
