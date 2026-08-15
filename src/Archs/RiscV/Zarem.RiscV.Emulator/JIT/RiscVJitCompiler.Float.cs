// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.RiscV.Models.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
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
        // Resolve the method to call based on the method name
        var method = typeof(TFormat).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        Guard.IsNotNull(method);

        EmitStoreRegister<TFormat>(il, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            il.Emit(OpCodes.Call, method);
        });
    }

    private void FloatMinMax<TFormat>(ILGenerator il, RiscVFloatInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        // If the instruction is not a float min or max, make an illegal instruction trap
        if (inst.Funct3 is not (FloatFunct3Code.FloatMin or FloatFunct3Code.FloatMax))
            IllegalInstruction(il, inst, pc, compressed);

        // Resolve the method to call based on the instruction's funct3
        var method = inst.Funct3 switch
        {
            FloatFunct3Code.FloatMin => typeof(TFormat).GetMethod(nameof(TFormat.Min), BindingFlags.Public | BindingFlags.Static),
            FloatFunct3Code.FloatMax => typeof(TFormat).GetMethod(nameof(TFormat.Max), BindingFlags.Public | BindingFlags.Static),
            _ => throw new NotSupportedException($"Unsupported float function: {inst.Funct3}"),
        };
        Guard.IsNotNull(method);

        EmitStoreRegister<TFormat>(il, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(il, inst.FRS1);
            EmitLoadRegister<TFormat>(il, inst.FRS2);
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

    private void FloatMacGuffin<TFormat>(ILGenerator il, RiscVFloatInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        Action<ILGenerator, RiscVInstruction, T, bool> func = inst.FRS2 switch
        {
            0 when inst.Funct3 is FloatFunct3Code.FloatMoveFrom => IllegalInstruction,
            0 when inst.Funct3 is FloatFunct3Code.FloatClassify => IllegalInstruction,
            _ => FloatConvertTo<TFormat>
        };

        func(il, inst, pc, compressed);
    }

    private void FloatConvertFrom<TFormat>(ILGenerator il, RiscVFloatInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        Action<ILGenerator, RiscVInstruction, T, bool> func = inst.IntFormat switch
        {
            RiscVIntFormat.Word => FloatConvertFrom<TFormat, int>,
            RiscVIntFormat.WordUnsigned => FloatConvertFrom<TFormat, uint>,
            RiscVIntFormat.Long => FloatConvertFrom<TFormat, long>,
            RiscVIntFormat.LongUnsigned => FloatConvertFrom<TFormat, ulong>,
            _ => IllegalInstruction,
        };

        func(il, inst, pc, compressed);
    }

    private void FloatConvertTo<TTo>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        var fInst = (RiscVFloatInstruction)inst;

        Action<ILGenerator, RiscVInstruction, T, bool> func = fInst.IntFormat switch
        {
            RiscVIntFormat.Word => FloatConvertTo<int, TTo>,
            RiscVIntFormat.WordUnsigned => FloatConvertTo<uint, TTo>,
            RiscVIntFormat.Long => FloatConvertTo<long, TTo>,
            RiscVIntFormat.LongUnsigned => FloatConvertTo<ulong, TTo>,
            _ => IllegalInstruction,
        };

        func(il, inst, pc, compressed);
    }

    private void FloatConvertFrom<TFrom, TTo>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
        where TTo : unmanaged, INumber<TTo>, IMinMaxValue<TTo>
    {
        var fInst = (RiscVFloatInstruction)inst;

        // Resolve mode and methods
        var mode = ResolveRoundingMode(fInst.RoundingMode);
        var round = typeof(TFrom).GetMethod(nameof(TFrom.Round), [typeof(TFrom), typeof(MidpointRounding)]);
        var createTruncatingDef = typeof(TTo)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == nameof(TTo.CreateTruncating) && m.IsGenericMethod);
        var convert = createTruncatingDef.MakeGenericMethod(typeof(TFrom));

        Guard.IsNotNull(round);
        Guard.IsNotNull(convert);

        EmitStoreRegister(il, inst.RD, _ =>
        {
            // Load the source floating-point register
            EmitLoadRegister<TFrom>(il, fInst.FRS1);

            // TODO: Handle special cases for NaN, Infinity, and overflow according to the RISC-V specification.

            // Round
            il.EmitLoadConstant((int)mode);
            il.Emit(OpCodes.Call, round);
            
            // Convert
            il.Emit(OpCodes.Call, convert);
        });
    }

    private void FloatConvertTo<TFrom, TTo>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TFrom : unmanaged, INumber<TFrom>, IMinMaxValue<TFrom>
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        // Resolve the method to call for conversion
        var createTruncatingDef = typeof(TTo)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(TTo.CreateTruncating) && m.IsGenericMethod);
        var convert = createTruncatingDef.MakeGenericMethod(typeof(TFrom));
        Guard.IsNotNull(convert);

        EmitStoreRegister<TTo>(il, ((RiscVFloatInstruction)inst).FRD, () =>
        {
            EmitLoadRegister<TFrom>(il, inst.RS1);
            il.Emit(OpCodes.Call, convert);
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

    private static MidpointRounding ResolveRoundingMode(RiscVRoundingMode rm)
    {
        return rm switch
        {
            RiscVRoundingMode.RoundToNearestEven => MidpointRounding.ToEven,
            RiscVRoundingMode.RoundTowardsZero => MidpointRounding.ToZero,
            RiscVRoundingMode.RoundDown => MidpointRounding.ToNegativeInfinity,
            RiscVRoundingMode.RoundUp => MidpointRounding.ToPositiveInfinity,
            RiscVRoundingMode.RoundToNearestMaxMagnitude => MidpointRounding.AwayFromZero,
            RiscVRoundingMode.Dynamic => MidpointRounding.ToEven, // TODO: Handle CSR register default
            _ => throw new InvalidOperationException()
        };
    }
}
