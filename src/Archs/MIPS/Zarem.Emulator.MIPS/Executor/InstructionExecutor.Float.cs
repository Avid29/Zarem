// Avishai Dernis 2026

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;
using static Zarem.Emulator.Machine.CPU.CoProcessors.FloatProcessor;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial class InstructionExecutor
{
    private Execution CreateCoproc1Execution()
    {
        return FloatInstruction.CoProc1RSCode switch
        {
            CoProc1RSCode.MFC1 => Execution.CreateWriteback(FloatInstruction.RT, Processor.FloatProcessor[FloatInstruction.FS]),
            CoProc1RSCode.CFC1 => throw new NotImplementedException(),
            CoProc1RSCode.MFHC1 => throw new NotImplementedException(),
            CoProc1RSCode.MTC1 => Execution.CreateFloatWriteback(FloatInstruction.FS, Processor[FloatInstruction.RT]),
            CoProc1RSCode.CTC1 => throw new NotImplementedException(),
            CoProc1RSCode.MTHC1 => throw new NotImplementedException(),
            CoProc1RSCode.BC1 => throw new NotImplementedException(),
            CoProc1RSCode.BC1ANY2 => throw new NotImplementedException(),
            CoProc1RSCode.BC1ANY4 => throw new NotImplementedException(),
            CoProc1RSCode.BZ_V => throw new NotImplementedException(),
            CoProc1RSCode.BC1NEZ => throw new NotImplementedException(),
            CoProc1RSCode.BNZ_V => throw new NotImplementedException(),
            CoProc1RSCode.BZ_B => throw new NotImplementedException(),
            CoProc1RSCode.BZ_H => throw new NotImplementedException(),
            CoProc1RSCode.BZ_W => throw new NotImplementedException(),
            CoProc1RSCode.BZ_D => throw new NotImplementedException(),
            CoProc1RSCode.BNZ_B => throw new NotImplementedException(),
            CoProc1RSCode.BNZ_H => throw new NotImplementedException(),
            CoProc1RSCode.BNZ_W => throw new NotImplementedException(),
            CoProc1RSCode.BNZ_D => throw new NotImplementedException(),

            _ => FloatInstruction.Format switch
            {
                FloatFormat.Single => CreateFloatExecution(Processor.FloatProcessor.Singles),
                FloatFormat.Double => CreateFloatExecution(Processor.FloatProcessor.Doubles),
                FloatFormat.Word => CreateFloatIntExecution(Processor.FloatProcessor.Words),
                FloatFormat.Long => CreateFloatIntExecution(Processor.FloatProcessor.Longs),
                _ => throw new NotImplementedException(),
            }
        };
    }

    private Execution CreateFloatExecution<T>(IFloatRegisterIndexer<T> indexer)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        return FloatInstruction.FloatFuncCode switch
        {
            FloatFuncCode.ConvertToDouble => CreateConvertExecution<T, double>(indexer),
            FloatFuncCode.ConvertToSingle => CreateConvertExecution<T, float>(indexer),
            FloatFuncCode.ConvertToWord => CreateConvertExecution<T, int>(indexer),
            FloatFuncCode.ConvertToLong => CreateConvertExecution<T, long>(indexer),

            FloatFuncCode.Round_L or FloatFuncCode.Truncate_L or
            FloatFuncCode.Ceiling_L or FloatFuncCode.Floor_L => CreateFloatRoundExecution<T, long>(indexer),

            FloatFuncCode.Round_W or FloatFuncCode.Truncate_W or
            FloatFuncCode.Ceiling_W or FloatFuncCode.Floor_W => CreateFloatRoundExecution<T, int>(indexer),

            _ => CreateFloatArithmeticExecution(indexer)
        };
    }

    private Execution CreateFloatIntExecution<T>(IFloatRegisterIndexer<T> indexer)
        where T : unmanaged, INumber<T>
    {
        return FloatInstruction.FloatFuncCode switch
        {
            FloatFuncCode.ConvertToDouble => CreateConvertExecution<T, double>(indexer),
            FloatFuncCode.ConvertToSingle => CreateConvertExecution<T, float>(indexer),
            FloatFuncCode.ConvertToWord => CreateConvertExecution<T, int>(indexer),
            FloatFuncCode.ConvertToLong => CreateConvertExecution<T, long>(indexer),
            _ => throw new NotImplementedException(),
        };
    }

    private Execution CreateFloatRoundExecution<TFrom, TTo>(IFloatRegisterIndexer<TFrom> indexer)
        where TFrom : unmanaged, IFloatingPointIeee754<TFrom>
        where TTo : INumber<TTo>, IMinMaxValue<TTo>
    {
        var destination = FloatInstruction.FD;

        // Retrieve the values from the register file
        var fs = indexer[FloatInstruction.FS];

        var rounded = FloatInstruction.FloatFuncCode switch
        {
            FloatFuncCode.Round_L or FloatFuncCode.Round_W => TFrom.Round(fs, MidpointRounding.ToEven),
            FloatFuncCode.Truncate_L or FloatFuncCode.Truncate_W => TFrom.Truncate(fs),
            FloatFuncCode.Ceiling_L or FloatFuncCode.Ceiling_W => TFrom.Ceiling(fs),
            FloatFuncCode.Floor_L or FloatFuncCode.Floor_W => TFrom.Floor(fs),

            _ => throw new NotImplementedException($"FPU instruction {FloatInstruction.FloatFuncCode} not implemented."),
        };

        // MIPS behavior: Handle out-of-range values before they hit the RegisterFile
        TTo finalResult;

        // Check if the rounded value fits in the target integer type
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


        return Execution.CreateFloatWriteback(destination, finalResult);
    }

    private Execution CreateFloatArithmeticExecution<T>(IFloatRegisterIndexer<T> indexer)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
        var destination = FloatInstruction.FD;

        // Retrieve the values from the register file
        var fs = indexer[FloatInstruction.FS];
        var ft = indexer[FloatInstruction.FT];

        var value = FloatInstruction.FloatFuncCode switch
        {
            FloatFuncCode.Add => fs + ft,
            FloatFuncCode.Subtract => fs - ft,
            FloatFuncCode.Multiply => fs * ft,
            FloatFuncCode.Divide => fs / ft,
            FloatFuncCode.SquareRoot => T.Sqrt(fs),
            FloatFuncCode.AbsoluteValue => T.Abs(fs),
            FloatFuncCode.Move => fs,
            FloatFuncCode.Negate => -fs,

            FloatFuncCode.Reciprical => T.ReciprocalEstimate(fs),

            _ => throw new NotImplementedException($"FPU instruction {FloatInstruction.FloatFuncCode} not implemented."),
        };

        return Execution.CreateFloatWriteback(destination, value);
    }

    private Execution CreateConvertExecution<TFrom, TTo>(IFloatRegisterIndexer<TFrom> indexer)
        where TFrom : INumber<TFrom>
        where TTo : INumber<TTo>
    {
        var source = indexer[FloatInstruction.FS];
        var result = TTo.CreateTruncating(source);
        return Execution.CreateFloatWriteback(FloatInstruction.FD, result);
    }
}
