// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T, TFloat>
{
    private bool FloatAlu<TFormat>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister<TFormat>(context, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(context, inst.FRS1);
            EmitLoadRegister<TFormat>(context, inst.FRS2);
            context.ILGenerator.Emit(ilOpCode);
        });

        return false;
    }

    private bool FloatUnary<TFormat>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatInstruction inst, T pc, string methodName)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        // Resolve the method to call based on the method name
        var method = typeof(TFormat).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        Guard.IsNotNull(method);

        EmitStoreRegister<TFormat>(context, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(context, inst.FRS1);
            context.ILGenerator.Emit(OpCodes.Call, method);
        });

        return false;
    }

    private bool FloatMinMax<TFormat>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        // If the instruction is not a float min or max, make an illegal instruction trap
        if (inst.Funct3 is not (FloatFunct3Code.FloatMin or FloatFunct3Code.FloatMax))
            IllegalInstruction(context, inst, pc, compressed);

        // Resolve the method to call based on the instruction's funct3
        var method = inst.Funct3 switch
        {
            FloatFunct3Code.FloatMin => typeof(TFormat).GetMethod(nameof(TFormat.Min), BindingFlags.Public | BindingFlags.Static),
            FloatFunct3Code.FloatMax => typeof(TFormat).GetMethod(nameof(TFormat.Max), BindingFlags.Public | BindingFlags.Static),
            _ => throw new NotSupportedException($"Unsupported float function: {inst.Funct3}"),
        };
        Guard.IsNotNull(method);

        EmitStoreRegister<TFormat>(context, inst.FRD, () =>
        {
            EmitLoadRegister<TFormat>(context, inst.FRS1);
            EmitLoadRegister<TFormat>(context, inst.FRS2);
            context.ILGenerator.Emit(OpCodes.Call, method);
        });

        return false;
    }

    private bool FloatCompare<TFormat>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatInstruction inst, OpCode ilOpCode)
        where TFormat : unmanaged
    {
        EmitStoreRegister(context, ((RiscVInstruction)inst).RD, _ =>
        {
            EmitLoadRegister<TFormat>(context, inst.FRS1);
            EmitLoadRegister<TFormat>(context, inst.FRS2);
            context.ILGenerator.Emit(ilOpCode);
        });

        return false;
    }

    private bool FloatFle<TFormat>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatInstruction inst)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        // No direct IL opcode for floating-point less than or equal,
        // so we use a combination of comparison and logical operations

        EmitStoreRegister(context, ((RiscVInstruction)inst).RD, _ =>
        {
            EmitLoadRegister<TFormat>(context, inst.FRS1);
            EmitLoadRegister<TFormat>(context, inst.FRS2);

            // Compare great than unsigned,
            // then negate the result to get less than or equal
            context.ILGenerator.Emit(OpCodes.Cgt_Un);
            context.ILGenerator.EmitLoadConstant(0);
            context.ILGenerator.Emit(OpCodes.Ceq);
        });

        return false;
    }

    private bool FloatMacGuffin<TFormat>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        Func<JitBlockContext<T, RiscVGpRegister, RiscVTrap>, RiscVInstruction, T, bool, bool> func = inst.FRS2 switch
        {
            0 when inst.Funct3 is FloatFunct3Code.FloatMoveFrom => IllegalInstruction,
            0 when inst.Funct3 is FloatFunct3Code.FloatClassify => IllegalInstruction,
            _ => FloatConvertTo<TFormat>
        };

        return func(context, inst, pc, compressed);
    }

    private bool FloatConvertFrom<TFormat>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        Func<JitBlockContext<T, RiscVGpRegister, RiscVTrap>, RiscVInstruction, T, bool, bool> func = inst.IntFormat switch
        {
            RiscVIntFormat.Word => FloatConvertFrom<TFormat, int>,
            RiscVIntFormat.WordUnsigned => FloatConvertFrom<TFormat, uint>,
            RiscVIntFormat.Long => FloatConvertFrom<TFormat, long>,
            RiscVIntFormat.LongUnsigned => FloatConvertFrom<TFormat, ulong>,
            _ => IllegalInstruction,
        };

        return func(context, inst, pc, compressed);
    }

    private bool FloatConvertTo<TTo>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        var fInst = (RiscVFloatInstruction)inst;

        Func<JitBlockContext<T, RiscVGpRegister, RiscVTrap>, RiscVInstruction, T, bool, bool> func = fInst.IntFormat switch
        {
            RiscVIntFormat.Word => FloatConvertTo<int, TTo>,
            RiscVIntFormat.WordUnsigned => FloatConvertTo<uint, TTo>,
            RiscVIntFormat.Long => FloatConvertTo<long, TTo>,
            RiscVIntFormat.LongUnsigned => FloatConvertTo<ulong, TTo>,
            _ => IllegalInstruction,
        };

        return func(context, inst, pc, compressed);
    }

    private bool FloatConvertFrom<TFrom, TTo>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
        where TTo : unmanaged, INumber<TTo>, IMinMaxValue<TTo>
    {
        var il = context.ILGenerator;
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

        EmitStoreRegister(context, inst.RD, _ =>
        {
            // Load the source floating-point register
            EmitLoadRegister<TFrom>(context, fInst.FRS1);

            // TODO: Handle special cases for NaN, Infinity, and overflow according to the RISC-V specification.

            // Round
            il.EmitLoadConstant((int)mode);
            il.Emit(OpCodes.Call, round);
            
            // Convert
            il.Emit(OpCodes.Call, convert);
        });
        return false;
    }

    private bool FloatConvertTo<TFrom, TTo>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TFrom : unmanaged, INumber<TFrom>, IMinMaxValue<TFrom>
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        // Resolve the method to call for conversion
        var createTruncatingDef = typeof(TTo)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(TTo.CreateTruncating) && m.IsGenericMethod);
        var convert = createTruncatingDef.MakeGenericMethod(typeof(TFrom));
        Guard.IsNotNull(convert);

        EmitStoreRegister<TTo>(context, ((RiscVFloatInstruction)inst).FRD, () =>
        {
            EmitLoadRegister<TFrom>(context, inst.RS1);
            context.ILGenerator.Emit(OpCodes.Call, convert);
        });
        return false;
    }

    private void EmitLoadRegister<TFloatData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatRegister register)
        where TFloatData : unmanaged
    {
        var il = context.ILGenerator;

        // Load the register's address then retrieve the value at that address
        EmitLoadRegisterAddress(il, register);
        il.EmitLdind<TFloatData>();
    }

    private void EmitStoreRegister<TFloatData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVFloatRegister register, Action emitEvaluation)
        where TFloatData : unmanaged
    {
        var il = context.ILGenerator;

        // Load the register's address, emit the evaluation instructions, and store the value
        EmitLoadRegisterAddress(il, register);
        emitEvaluation();
        il.EmitStind<TFloatData>();
    }
    
    private void EmitLoadRegisterAddress(ILGenerator il, RiscVFloatRegister register)
    {
        Guard.IsNotNull(Cpu.FloatRegisterFile);

        EmitLoadRegisterAddress(il, (int)register, Cpu.FloatRegisterFile.Regs);
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
