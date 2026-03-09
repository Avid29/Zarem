// Avishai Dernis 2026

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
            CoProc1RSCode.MFC1 => throw new NotImplementedException(),
            CoProc1RSCode.CFC1 => throw new NotImplementedException(),
            CoProc1RSCode.MFHC1 => throw new NotImplementedException(),
            CoProc1RSCode.MTC1 => throw new NotImplementedException(),
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
                _ => throw new NotImplementedException(),
            }
        };
    }

    private Execution CreateFloatExecution<T>(IFloatRegisterIndexer<T> indexer)
        where T : unmanaged, IFloatingPointIeee754<T>
    {
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

            // Approximation
            FloatFuncCode.Reciprical => T.One / fs,

            // Rounding to Long (64-bit integer)
            FloatFuncCode.Round_L => T.CreateTruncating(T.Round(fs, MidpointRounding.ToEven)),
            FloatFuncCode.Truncate_L => T.CreateTruncating(T.Truncate(fs)),
            FloatFuncCode.Ceiling_L => T.CreateTruncating(T.Ceiling(fs)),
            FloatFuncCode.Floor_L => T.CreateTruncating(T.Floor(fs)),

            // Rounding to Word (32-bit integer)
            FloatFuncCode.Round_W => T.CreateTruncating(T.Round(fs, MidpointRounding.ToEven)),
            FloatFuncCode.Truncate_W => T.CreateTruncating(T.Truncate(fs)),
            FloatFuncCode.Ceiling_W => T.CreateTruncating(T.Ceiling(fs)),
            FloatFuncCode.Floor_W => T.CreateTruncating(T.Floor(fs)),

            // Type Conversions
            // Note: These usually involve switching the 'Format' in the switch above this one.
            // If the instruction is CVT.S.D, 'T' is float, and we convert 'fs' (which is double).
            // For simplicity in a generic method, we cast from the input to T.
            //FloatFuncCode.ConvertToSingle => T.CreateTruncating(fs),
            //FloatFuncCode.ConvertToDouble => T.CreateTruncating(fs),
            //FloatFuncCode.ConvertToWord => T.CreateTruncating(fs),
            //FloatFuncCode.ConvertToLong => T.CreateTruncating(fs),

            _ => throw new NotImplementedException($"FPU instruction {FloatInstruction.FloatFuncCode} not implemented."),
        };

        var destination = FloatInstruction.FD;

        Span<byte> buffer = stackalloc byte[8];
        buffer.Clear(); // Ensure high bits are 0 for 32-bit floats
        MemoryMarshal.Write(buffer, in value);
        ulong longValue = MemoryMarshal.Read<ulong>(buffer);

        // TODO: Add execution constructor for double coproc writebacks
        return new Execution
        {
            FloatReg = destination,
            Low = (uint)(longValue & 0xFFFF_FFFF),
            High = (uint)(longValue >> 32),
            SideEffect = SideEffect.WriteCoProc,
        };
    }
}
