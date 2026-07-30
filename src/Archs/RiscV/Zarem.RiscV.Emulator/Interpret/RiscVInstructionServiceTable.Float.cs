// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.Machine.Registers;
using Zarem.RiscV.Emulator.Enums;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.Emulator.Models;

public unsafe partial class RiscVInstructionServiceTable<T, TFloat, TSigned>
{
    private static RiscVTrap FloatAlu<TLogic, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];
        var frs2 = indexer[(int)inst.FRS2];
        var value = TLogic.Compute(frs1, frs2);
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, value);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatFAlu<TLogic, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IFAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];
        var value = TLogic.Compute(frs1);
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, value);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatMinMax<TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];
        var frs2 = indexer[(int)inst.FRS2];
        TFormat value;
        switch (inst.Funct3)
        {
            case FloatFunct3Code.FloatMin:
                value = TFormat.Min(frs1, frs2);
                break;
            case FloatFunct3Code.FloatMax:
                value = TFormat.Max(frs1, frs2);
                break;
            default:
                return IllegalInstruction(cpu, inst, out exec);
        }
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, value);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatCompare<TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];
        var frs2 = indexer[(int)inst.FRS2];
        bool compare;
        switch (inst.Funct3)
        {
            case FloatFunct3Code.FloatLessOrEqual:
                compare = frs1 <= frs2;
                break;
            case FloatFunct3Code.FloatLessThan:
                compare = frs1 < frs2;
                break;
            case FloatFunct3Code.FloatEqual:
                compare = frs1 == frs2;
                break;
            default:
                return IllegalInstruction(cpu, inst, out exec);
        }

        exec = RiscVExecution<T>.CreateWriteback(((RiscVInstruction)inst).RD, compare ? T.One : T.Zero);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatMacGuffin<TTo>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        return (byte)inst.FRS2 switch
        {
            0 when inst.Funct3 is FloatFunct3Code.FloatMoveFrom => FloatMoveTo<TTo>(cpu, inst, out exec),
            0 when inst.Funct3 is FloatFunct3Code.FloatClassify => FloatClassifiy<TTo>(cpu, inst, out exec),
            _ =>  FloatConvertTo<TTo>(cpu, inst, out exec),
        };
    }

    private static RiscVTrap FloatClassifiy<TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];

        var classification = FloatClassification.None;

        // Check each condition based on the RISC-V specification for fclass
        if (TFormat.IsNegative(frs1))
        {
            if (TFormat.IsInfinity(frs1)) classification = FloatClassification.NegativeInfinity;
            else if (TFormat.IsNormal(frs1)) classification = FloatClassification.NegativeNormal;
            else if (TFormat.IsSubnormal(frs1)) classification = FloatClassification.NegativeSubnormal;
            else if (TFormat.IsZero(frs1)) classification = FloatClassification.NegativeZero;
        }
        else if (TFormat.IsPositive(frs1))
        {
            if (TFormat.IsInfinity(frs1)) classification = FloatClassification.PositiveInfinity;
            else if (TFormat.IsNormal(frs1)) classification = FloatClassification.PositiveNormal;
            else if (TFormat.IsSubnormal(frs1)) classification = FloatClassification.PositiveSubnormal;
            else if (TFormat.IsZero(frs1)) classification = FloatClassification.PositiveZero;
        }
        else if (TFormat.IsNaN(frs1))
        {
            // TODO: Differentiate signaling/quiet NaN
            classification = FloatClassification.QuietNaN;
        }

        exec = RiscVExecution<T>.CreateWriteback(((RiscVInstruction)inst).RD, T.CreateTruncating((short)classification));
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatMoveTo<TTo>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        var source = cpu.RegisterFile.Regs[(int)((RiscVInstruction)inst).RS1];
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, source);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatMoveFrom<TFrom>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
    {
#if DEBUG
        Guard.IsNotNull(cpu.FloatRegisterFile);
#endif

        var source = cpu.FloatRegisterFile.Regs[(int)((RiscVInstruction)inst).RS1];
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, source);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatConvertTo<TTo>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        delegate*<RiscVInterpretCpu<T, TFloat>, RiscVFloatInstruction, out RiscVExecution<T>, RiscVTrap> func = inst.IntFormat switch
        {
            RiscVIntFormat.Word => &FloatConvertTo<int, TTo>,
            RiscVIntFormat.WordUnsigned => &FloatConvertTo<uint, TTo>,
            RiscVIntFormat.Long => &FloatConvertTo<long, TTo>,
            RiscVIntFormat.LongUnsigned => &FloatConvertTo<ulong, TTo>,
            _ => &IllegalInstruction,
        };

        return func(cpu, inst, out exec);
    }

    private static RiscVTrap FloatConvertFrom<TFrom>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
    {
        delegate*<RiscVInterpretCpu<T, TFloat>, RiscVFloatInstruction, out RiscVExecution<T>, RiscVTrap> func = inst.IntFormat switch
        {
            RiscVIntFormat.Word => &FloatConvertFrom<TFrom, int>,
            RiscVIntFormat.WordUnsigned => &FloatConvertFrom<TFrom, uint>,
            RiscVIntFormat.Long => &FloatConvertFrom<TFrom, long>,
            RiscVIntFormat.LongUnsigned => &FloatConvertFrom<TFrom, ulong>,
            _ => &IllegalInstruction,
        };

        return func(cpu, inst, out exec);
    }

    private static RiscVTrap FloatConvertTo<TFrom, TTo>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        var source = TFrom.CreateTruncating(cpu.RegisterFile.Regs[(int)((RiscVInstruction)inst).RS1]);
        var result = TTo.CreateTruncating(source);
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, result);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatConvertFrom<TFrom, TTo>(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
        where TTo : unmanaged, INumber<TTo>, IMinMaxValue<TTo>
    {
        var indexer = GetFloatRegisterIndexer<TFrom>(cpu);
        var source = indexer[(int)inst.FRS1];

        TTo result;

        if (TFrom.IsNaN(source))
        {
            result = TTo.MaxValue;

            // TODO: Accumulate Invalid Operation flag (NV) in CSR here if mimicking hardware exceptions
        }
        else
        {
            var mode = ResolveRoundingMode(inst.RoundingMode);
            var rounded = TFrom.Round(source, mode);
            result = TTo.CreateTruncating(rounded);
        }

        exec = RiscVExecution<T>.CreateWriteback(((RiscVInstruction)inst).RD, T.CreateTruncating(result));
        return RiscVTrap.None;
    }

    private static RiscVTrap IllegalInstruction(RiscVInterpretCpu<T, TFloat> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        => IllegalInstruction(cpu, inst, out exec);

    private static IFormattedRegisterIndexer<TFormat> GetFloatRegisterIndexer<TFormat>(RiscVInterpretCpu<T, TFloat> cpu)
        where TFormat : unmanaged, INumber<TFormat>
    {
#if DEBUG
        Guard.IsNotNull(cpu.FloatRegisterFile);
#endif

        if (typeof(TFormat) == typeof(Half)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatRegisterFile.Halves;
        else if (typeof(TFormat) == typeof(float)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatRegisterFile.Singles;
        else if (typeof(TFormat) == typeof(double)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatRegisterFile.Doubles;
        else throw new InvalidOperationException();
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
