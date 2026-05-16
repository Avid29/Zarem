// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.Machine.Registers;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.Emulator.Models;

public unsafe partial class RiscVInstructionServiceTable<T, TSigned>
{
    private static RiscVTrap FloatAlu<TLogic, TFormat>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
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

    private static RiscVTrap FloatFAlu<TLogic, TFormat>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IFAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];
        var value = TLogic.Compute(frs1);
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, value);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatMinMax<TFormat>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
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

    private static RiscVTrap FloatCompare<TFormat>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
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

    private enum Classification : ushort
    {
        None = 0x0,
        NegativeInfinity = 0x1,
        NegativeNormal = 0x2,
        NegativeSubnormal = 0x4,
        NegativeZero = 0x8,
        PositiveZero = 0x10,
        PositiveSubnormal = 0x20,
        PositiveNormal = 0x40,
        PositiveInfinity = 0x80,
        SignalingNaN = 0x100,
        QuietNaN = 0x200,
    };

    private static RiscVTrap FloatClassifiy<TFormat>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];

        Classification classification = Classification.None;

        // Check each condition based on the RISC-V specification for fclass
        if (TFormat.IsNegative(frs1))
        {
            if (TFormat.IsInfinity(frs1)) classification = Classification.NegativeInfinity;
            else if (TFormat.IsNormal(frs1)) classification = Classification.NegativeNormal;
            else if (TFormat.IsSubnormal(frs1)) classification = Classification.NegativeSubnormal;
            else if (TFormat.IsZero(frs1)) classification = Classification.NegativeZero;
        }
        else if (TFormat.IsPositive(frs1))
        {
            if (TFormat.IsInfinity(frs1)) classification = Classification.PositiveInfinity;
            else if (TFormat.IsNormal(frs1)) classification = Classification.PositiveNormal;
            else if (TFormat.IsSubnormal(frs1)) classification = Classification.PositiveSubnormal;
            else if (TFormat.IsZero(frs1)) classification = Classification.PositiveZero;
        }
        else if (TFormat.IsNaN(frs1))
        {
            // TODO: Differentiate signaling/quiet NaN
            classification = Classification.QuietNaN;
        }

        exec = RiscVExecution<T>.CreateWriteback(((RiscVInstruction)inst).RD, T.CreateTruncating((short)classification));
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatConvertTo<TTo>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        delegate*<RiscVInterpretCpu<T>, RiscVFloatInstruction, out RiscVExecution<T>, RiscVTrap> func = inst.IntFormat switch
        {
            RiscVIntFormat.Word => &FloatConvertTo<int, TTo>,
            RiscVIntFormat.WordUnsigned => &FloatConvertTo<uint, TTo>,
            RiscVIntFormat.Long => &FloatConvertTo<long, TTo>,
            RiscVIntFormat.LongUnsigned => &FloatConvertTo<ulong, TTo>,
            _ => &IllegalInstruction,
        };

        return func(cpu, inst, out exec);
    }

    private static RiscVTrap FloatConvertFrom<TFrom>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
    {
        delegate*<RiscVInterpretCpu<T>, RiscVFloatInstruction, out RiscVExecution<T>, RiscVTrap> func = inst.IntFormat switch
        {
            RiscVIntFormat.Word => &FloatConvertFrom<TFrom, int>,
            RiscVIntFormat.WordUnsigned => &FloatConvertFrom<TFrom, uint>,
            RiscVIntFormat.Long => &FloatConvertFrom<TFrom, long>,
            RiscVIntFormat.LongUnsigned => &FloatConvertFrom<TFrom, ulong>,
            _ => &IllegalInstruction,
        };

        return func(cpu, inst, out exec);
    }

    private static RiscVTrap FloatConvertTo<TFrom, TTo>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, IBinaryFloatingPointIeee754<TTo>
    {
        var source = TFrom.CreateTruncating(cpu.RegisterFile.Regs[(int)((RiscVInstruction)inst).RS1]);
        var result = TTo.CreateTruncating(source);
        exec = RiscVExecution<T>.CreateFloatWriteback(inst.FRD, result);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatConvertFrom<TFrom, TTo>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
        where TTo : unmanaged, INumber<TTo>, IMinMaxValue<TTo>
    {
        var indexer = GetFloatRegisterIndexer<TFrom>(cpu);
        var source = indexer[(int)inst.FRS1];

        TTo result;

        if (TFrom.IsNaN(source))
        {
            result = TTo.MaxValue;
        }
        else
        {
            result = TTo.CreateTruncating(source);
        }

        exec = RiscVExecution<T>.CreateWriteback(((RiscVInstruction)inst).RD, T.CreateTruncating(result));
        return RiscVTrap.None;
    }

    private static RiscVTrap IllegalInstruction(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        => IllegalInstruction(cpu, inst, out exec);

    private static IFormattedRegisterIndexer<TFormat> GetFloatRegisterIndexer<TFormat>(RiscVInterpretCpu<T> cpu)
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
}
