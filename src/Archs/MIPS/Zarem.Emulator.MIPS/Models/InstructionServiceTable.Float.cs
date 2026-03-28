// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Emulator.Models;

public partial class InstructionServiceTable<T, TSigned>
{
    private static MipsTrap CreateCoProc1Execution(InstructionServiceTable<T, TSigned> @this, MipsInstruction inst, out Execution<T> exec)
    {
        var floatInstruction = (FloatInstruction)inst;

        exec = floatInstruction.CoProc1RSCode switch
        {
            CoProc1RSCode.MFC1 => Execution<T>.CreateWriteback(floatInstruction.RT, T.CreateTruncating(@this._processor.FloatProcessor[floatInstruction.FS])),
            CoProc1RSCode.CFC1 => throw new NotImplementedException(),
            CoProc1RSCode.MFHC1 => throw new NotImplementedException(),
            CoProc1RSCode.MTC1 => Execution<T>.CreateFloatWriteback(floatInstruction.FS, @this._processor[floatInstruction.RT]),
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

            _ => floatInstruction.Format switch
            {
                FloatFormat.Single => CreateFloatExecution(inst, @this._processor.FloatProcessor.Singles),
                FloatFormat.Double => CreateFloatExecution(inst, @this._processor.FloatProcessor.Doubles),
                FloatFormat.Word => CreateFloatIntExecution(inst, @this._processor.FloatProcessor.Words),
                FloatFormat.Long => CreateFloatIntExecution(inst, @this._processor.FloatProcessor.Longs),
                _ => throw new NotImplementedException(),
            }
        };

        return MipsTrap.None;
    }

    private static Execution<T> CreateFloatExecution<T2>(FloatInstruction inst, IFloatRegisterIndexer<T2> indexer)
        where T2 : unmanaged, IFloatingPointIeee754<T2>
    {
        return inst.FloatFuncCode switch
        {
            FloatFuncCode.ConvertToDouble => CreateConvertExecution<T2, double>(inst, indexer),
            FloatFuncCode.ConvertToSingle => CreateConvertExecution<T2, float>(inst, indexer),
            FloatFuncCode.ConvertToWord => CreateConvertExecution<T2, int>(inst, indexer),
            FloatFuncCode.ConvertToLong => CreateConvertExecution<T2, long>(inst, indexer),

            FloatFuncCode.Round_L or FloatFuncCode.Truncate_L or
            FloatFuncCode.Ceiling_L or FloatFuncCode.Floor_L => CreateFloatRoundExecution<T2, long>(inst, indexer),

            FloatFuncCode.Round_W or FloatFuncCode.Truncate_W or
            FloatFuncCode.Ceiling_W or FloatFuncCode.Floor_W => CreateFloatRoundExecution<T2, int>(inst, indexer),

            _ => CreateFloatArithmeticExecution(inst, indexer)
        };
    }

    private static Execution<T> CreateFloatIntExecution<T2>(FloatInstruction inst, IFloatRegisterIndexer<T2> indexer)
        where T2 : unmanaged, INumber<T2>
    {
        return inst.FloatFuncCode switch
        {
            FloatFuncCode.ConvertToDouble => CreateConvertExecution<T2, double>(inst, indexer),
            FloatFuncCode.ConvertToSingle => CreateConvertExecution<T2, float>(inst, indexer),
            FloatFuncCode.ConvertToWord => CreateConvertExecution<T2, int>(inst, indexer),
            FloatFuncCode.ConvertToLong => CreateConvertExecution<T2, long>(inst, indexer),
            _ => throw new NotImplementedException(),
        };
    }

    private static Execution<T> CreateFloatRoundExecution<TFrom, TTo>(FloatInstruction inst, IFloatRegisterIndexer<TFrom> indexer)
        where TFrom : unmanaged, IFloatingPointIeee754<TFrom>
        where TTo : unmanaged, INumber<TTo>, IMinMaxValue<TTo>
    {
        var destination = inst.FD;

        // Retrieve the values from the register file
        var fs = indexer[inst.FS];

        var rounded = inst.FloatFuncCode switch
        {
            FloatFuncCode.Round_L or FloatFuncCode.Round_W => TFrom.Round(fs, MidpointRounding.ToEven),
            FloatFuncCode.Truncate_L or FloatFuncCode.Truncate_W => TFrom.Truncate(fs),
            FloatFuncCode.Ceiling_L or FloatFuncCode.Ceiling_W => TFrom.Ceiling(fs),
            FloatFuncCode.Floor_L or FloatFuncCode.Floor_W => TFrom.Floor(fs),

            _ => throw new NotImplementedException($"FPU instruction {inst.FloatFuncCode} not implemented."),
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


        return Execution<T>.CreateFloatWriteback(destination, finalResult);
    }

    private static Execution<T> CreateFloatArithmeticExecution<T2>(FloatInstruction inst, IFloatRegisterIndexer<T2> indexer)
        where T2 : unmanaged, IFloatingPointIeee754<T2>
    {
        var destination = inst.FD;

        // Retrieve the values from the register file
        var fs = indexer[inst.FS];
        var ft = indexer[inst.FT];

        var value = inst.FloatFuncCode switch
        {
            FloatFuncCode.Add => fs + ft,
            FloatFuncCode.Subtract => fs - ft,
            FloatFuncCode.Multiply => fs * ft,
            FloatFuncCode.Divide => fs / ft,
            FloatFuncCode.SquareRoot => T2.Sqrt(fs),
            FloatFuncCode.AbsoluteValue => T2.Abs(fs),
            FloatFuncCode.Move => fs,
            FloatFuncCode.Negate => -fs,

            FloatFuncCode.Reciprical => T2.ReciprocalEstimate(fs),

            _ => throw new NotImplementedException($"FPU instruction {inst.FloatFuncCode} not implemented."),
        };

        return Execution<T>.CreateFloatWriteback(destination, value);
    }

    private static Execution<T> CreateConvertExecution<TFrom, TTo>(FloatInstruction inst, IFloatRegisterIndexer<TFrom> indexer)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        var source = indexer[inst.FS];
        var result = TTo.CreateTruncating(source);
        return Execution<T>.CreateFloatWriteback(inst.FD, result);
    }
}
