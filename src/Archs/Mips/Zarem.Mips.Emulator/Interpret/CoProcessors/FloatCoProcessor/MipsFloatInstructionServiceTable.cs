// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Logic;
using Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="MipsFloatExecution{T}"/> models.
/// </summary>
public unsafe partial class MipsFloatInstructionServiceTable<T, TFloat>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private readonly MipsFloatInstructionDecodeTable<IntPtr> _instructionTable;
    private readonly MipsCpu<T> _cpu;
    private readonly FloatCoProcessor<T, TFloat> _fpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionServiceTable{T, TS}"/> struct.
    /// </summary>
    public MipsFloatInstructionServiceTable(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu)
    {
        _cpu = cpu;
        _fpu = fpu;

        _instructionTable = new MipsFloatInstructionDecodeTable<IntPtr>(GetFunctionPtrValue(&ReservedInstruction));

        InitFloat(cpu.Config.VersionInfo);
    }

    /// <inheritdoc/>
    public MipsTrap Execute(MipsInstruction instruction, out MipsFloatExecution<T> execution)
    {
        var func = (delegate*<MipsCpu<T>, FloatCoProcessor<T, TFloat>, MipsFloatInstruction, out MipsFloatExecution<T>, MipsTrap>)_instructionTable.Lookup(instruction);
        return func(_cpu, _fpu, instruction, out execution);
    }

    private static MipsTrap FloatAlu<TLogic, TFormat>(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(fpu);
        var destination = inst.FD;
        var fs = indexer[(int)inst.FS];
        var ft = indexer[(int)inst.FT];
        var value = TLogic.Compute(fs, ft);
        exec = MipsFloatExecution<T>.CreateFloatWriteback(destination, value);
        return MipsTrap.None;
    }

    private static MipsTrap FloatFAlu<TLogic, TFormat>(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
        where TLogic : struct, IFAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(fpu);
        var destination = inst.FD;
        var fs = indexer[(int)inst.FS];
        var value = TLogic.Compute(fs);
        exec = MipsFloatExecution<T>.CreateFloatWriteback(destination, value);
        return MipsTrap.None;
    }

    private static MipsTrap FloatRound<TLogic, TFrom, TTo>(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
        where TLogic : struct, IRoundLogic<TFrom>
        where TFrom : unmanaged, IBinaryFloatingPointIeee754<TFrom>
        where TTo : unmanaged, IBinaryInteger<TTo>, IMinMaxValue<TTo>
    {
        var indexer = GetFloatRegisterIndexer<TFrom>(fpu);
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

        exec = MipsFloatExecution<T>.CreateFloatWriteback(inst.FD, finalResult);
        return MipsTrap.None;
    }

    private static MipsTrap FloatConvert<TFrom, TTo>(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        var indexer = GetFloatRegisterIndexer<TFrom>(fpu);
        var source = indexer[(int)inst.FS];
        var result = TTo.CreateTruncating(source);
        exec = MipsFloatExecution<T>.CreateFloatWriteback(inst.FD, result);
        return MipsTrap.None;
    }

    private static MipsTrap MFC1(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
    {
        var source = T.CreateTruncating(fpu[inst.FS]);
        exec = MipsExecution<T>.CreateWriteback(inst.RT, source);
        return MipsTrap.None;
    }

    private static MipsTrap MTC1(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
    {
        exec = MipsFloatExecution<T>.CreateFloatWriteback(inst.FS, cpu[inst.RT]);
        return MipsTrap.None;
    }

    private static MipsTrap CFC1(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(fpu.ControlRegisterFile[(int)inst.FS]));
        return MipsTrap.None;
    }

    private static MipsTrap CTC1(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsFloatInstruction inst, out MipsFloatExecution<T> exec)
    {
        exec = MipsFloatExecution<T>.CreateControlWriteback((CP1CRegisters)inst.FS, uint.CreateTruncating(cpu[inst.RT]));
        return MipsTrap.None;
    }

    private static MipsTrap ReservedInstruction(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = default;
        return MipsTrap.ReservedInstruction;
    }

    private static IFormattedRegisterIndexer<TFormat> GetFloatRegisterIndexer<TFormat>(FloatCoProcessor<T, TFloat> fpu)
        where TFormat : unmanaged, INumber<TFormat>
    {
        if (typeof(TFormat) == typeof(float)) return (IFormattedRegisterIndexer<TFormat>)fpu.Singles;
        else if (typeof(TFormat) == typeof(double)) return (IFormattedRegisterIndexer<TFormat>)fpu.Doubles;
        else if (typeof(TFormat) == typeof(int)) return (IFormattedRegisterIndexer<TFormat>)fpu.Words;
        else if (typeof(TFormat) == typeof(long)) return (IFormattedRegisterIndexer<TFormat>)fpu.Longs;
        else throw new InvalidOperationException();
    }
}
