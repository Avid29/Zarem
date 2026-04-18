// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Emulator.Models;

public unsafe partial class MipsInstructionServiceTable<T, TS>
{
    private static MipsTrap DispatchCoProc1(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var fInst = (FloatInstruction)inst;
        var func = @this._coProc1RSTable[(int)fInst.CoProc1RSCode];
        return func(@this, fInst, out exec);
    }

    private static MipsTrap CreateFloatExecution<T2>(MipsInstructionServiceTable<T, TS> @this, FloatInstruction inst, out MipsExecution<T> exec)
        where T2 : unmanaged, IFloatingPointIeee754<T2>
    {
        var indexer = @this.GetFloatRegisterIndexer<T2>();
        exec = inst.FloatFuncCode switch
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

        return MipsTrap.None;
    }

    private static MipsTrap CreateFloatIntExecution<T2>(MipsInstructionServiceTable<T, TS> @this, FloatInstruction inst, out MipsExecution<T> exec)
        where T2 : unmanaged, INumber<T2>
    {
        var indexer = @this.GetFloatRegisterIndexer<T2>();
        exec = inst.FloatFuncCode switch
        {
            FloatFuncCode.ConvertToDouble => CreateConvertExecution<T2, double>(inst, indexer),
            FloatFuncCode.ConvertToSingle => CreateConvertExecution<T2, float>(inst, indexer),
            FloatFuncCode.ConvertToWord => CreateConvertExecution<T2, int>(inst, indexer),
            FloatFuncCode.ConvertToLong => CreateConvertExecution<T2, long>(inst, indexer),
            _ => throw new NotImplementedException(),
        };

        return MipsTrap.None;
    }

    private static MipsTrap MFC1(MipsInstructionServiceTable<T, TS> @this, FloatInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(@this._processor.FloatProcessor[inst.FS]));
        return MipsTrap.None;
    }

    private static MipsTrap MTC1(MipsInstructionServiceTable<T, TS> @this, FloatInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateFloatWriteback(inst.FS, @this._processor[inst.RT]);
        return MipsTrap.None;
    }

    private static MipsExecution<T> CreateFloatRoundExecution<TFrom, TTo>(FloatInstruction inst, IFloatRegisterIndexer<TFrom> indexer)
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


        return MipsExecution<T>.CreateFloatWriteback(destination, finalResult);
    }

    private static MipsExecution<T> CreateFloatArithmeticExecution<T2>(FloatInstruction inst, IFloatRegisterIndexer<T2> indexer)
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
            FloatFuncCode.RecipricalSquareRoot => T2.ReciprocalSqrtEstimate(fs),

            _ => throw new NotImplementedException($"FPU instruction {inst.FloatFuncCode} not implemented."),
        };

        return MipsExecution<T>.CreateFloatWriteback(destination, value);
    }

    private static MipsExecution<T> CreateConvertExecution<TFrom, TTo>(FloatInstruction inst, IFloatRegisterIndexer<TFrom> indexer)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        var source = indexer[inst.FS];
        var result = TTo.CreateTruncating(source);
        return MipsExecution<T>.CreateFloatWriteback(inst.FD, result);
    }

    private IFloatRegisterIndexer<TFloat> GetFloatRegisterIndexer<TFloat>()
        where TFloat : unmanaged, INumber<TFloat>
    {
        if (typeof(TFloat) == typeof(float)) return (IFloatRegisterIndexer<TFloat>)_processor.FloatProcessor.Singles;
        else if (typeof(TFloat) == typeof(double)) return (IFloatRegisterIndexer<TFloat>)_processor.FloatProcessor.Doubles;
        else if (typeof(TFloat) == typeof(int)) return (IFloatRegisterIndexer<TFloat>)_processor.FloatProcessor.Words;
        else if (typeof(TFloat) == typeof(long)) return (IFloatRegisterIndexer<TFloat>)_processor.FloatProcessor.Longs;
        else throw new InvalidOperationException();
    }
}
