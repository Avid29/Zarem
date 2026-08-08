// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models;

public partial class MipsInstructionServiceTable<T, TFloat, TSigned>
{
    private static MipsTrap FloatAlu<TLogic, TFormat>(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var destination = inst.FD;
        var fs = indexer[(int)inst.FS];
        var ft = indexer[(int)inst.FT];
        var value = TLogic.Compute(fs, ft);
        exec = MipsExecution<T>.CreateFloatWriteback(destination, value);
        return MipsTrap.None;
    }

    private static MipsTrap FloatFAlu<TLogic, TFormat>(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IFAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var destination = inst.FD;
        var fs = indexer[(int)inst.FS];
        var value = TLogic.Compute(fs);
        exec = MipsExecution<T>.CreateFloatWriteback(destination, value);
        return MipsTrap.None;
    }

    private static MipsTrap FloatRound<TLogic, TFrom, TTo>(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IRoundLogic<TFrom>
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
        where TTo : unmanaged, IBinaryInteger<TTo>, IMinMaxValue<TTo>
    {
        var indexer = GetFloatRegisterIndexer<TFrom>(cpu);
        var source = indexer[(int)inst.FS];
        var rounded = TLogic.Compute(source);

        // MIPS behavior: Handle out-of-range values before they hit the RegisterFile
        // Check if the rounded value fits in the target integer type
        TTo finalResult;
        if (rounded > TFrom.CreateTruncating(TTo.MaxValue) ||
            rounded < TFrom.CreateTruncating(TTo.MinValue) ||
            TFrom.IsNaN(rounded))
        {
            // TODO: Log overflow

            // MIPS typically writes the most significant bits or a default 
            // for out-of-range conversions.
            finalResult = TTo.Zero;
        }
        else
        {
            finalResult = TTo.CreateTruncating(rounded);
        }

        exec = MipsExecution<T>.CreateFloatWriteback(inst.FD, finalResult);
        return MipsTrap.None;
    }

    private static MipsTrap FloatConvert<TFrom, TTo>(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        var indexer = GetFloatRegisterIndexer<TFrom>(cpu);
        var source = indexer[(int)inst.FS];
        var result = TTo.CreateTruncating(source);
        exec = MipsExecution<T>.CreateFloatWriteback(inst.FD, result);
        return MipsTrap.None;
    }

    private static MipsTrap MFC1(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(cpu.FloatProcessor[inst.FS]));
        return MipsTrap.None;
    }

    private static MipsTrap MTC1(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateFloatWriteback(inst.FS, cpu[inst.RT]);
        return MipsTrap.None;
    }

    private static MipsTrap CFC1(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(cpu.FloatProcessor.ControlRegisterFile[(int)inst.FS]));
        return MipsTrap.None;
    }

    private static MipsTrap CTC1(MipsInterpretCpu<T, TFloat> cpu, MipsFloatInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback((CP1CRegisters)inst.FS, cpu.FloatProcessor.ControlRegisterFile[(int)inst.RT]);
        return MipsTrap.None;
    }

    private static IFormattedRegisterIndexer<TFormat> GetFloatRegisterIndexer<TFormat>(MipsInterpretCpu<T, TFloat> cpu)
        where TFormat : unmanaged, INumber<TFormat>
    {
        if (typeof(TFormat) == typeof(float)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatProcessor.Singles;
        else if (typeof(TFormat) == typeof(double)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatProcessor.Doubles;
        else if (typeof(TFormat) == typeof(int)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatProcessor.Words;
        else if (typeof(TFormat) == typeof(long)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatProcessor.Longs;
        else throw new InvalidOperationException();
    }
}
