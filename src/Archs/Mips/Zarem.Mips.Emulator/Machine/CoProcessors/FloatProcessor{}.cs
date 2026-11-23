// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Interpret.CoProcessors;
using Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Registers.FloatProcessor;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// a class representing the floating-point coprocessor unit.
/// </summary>
public unsafe class FloatCoProcessor<T, TFloat> : IFloatCoProcessor<T>, IDisposable
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private readonly MipsFloatInstructionServiceTable<T, TFloat> _instructionServiceTable;
    private readonly MipsFloatJitCompiler<T, TFloat> _jitCompiler;
    private readonly IControlCoProcessor _co0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatCoProcessor{T, TFloat}"/> class.
    /// </summary>
    public FloatCoProcessor(MipsCpu<T> cpu)
    {
        _co0 = cpu.ControlCoProcessor;
        RegisterFile = new(32);
        ControlRegisterFile = new();

        _instructionServiceTable = new MipsFloatInstructionServiceTable<T, TFloat>(cpu, this);
        _jitCompiler = new MipsFloatJitCompiler<T, TFloat>(cpu, this);
    }

    internal FormattedRegisterFile<TFloat> RegisterFile { get; }

    /// <summary>
    /// Gets the float-point processor control register file.
    /// </summary>
    public MipsFloatControlRegisterFile ControlRegisterFile { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    public IFormattedRegisterIndexer<float> Singles => RegisterFile.Singles;

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as an <see cref="int"/>.
    /// </summary>
    public IFormattedRegisterIndexer<int> Words => RegisterFile.Words;

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="double"/>.
    /// </summary>
    public IFormattedRegisterIndexer<double> Doubles
    {
        get
        {
            bool use64BitMode = sizeof(TFloat) is sizeof(ulong) && _co0.FloatingPoint64BitMode;
            return use64BitMode
                ? RegisterFile.Doubles
                : new PairedDoubleIndexer(RegisterFile.Regs);
        }
    }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="long"/>.
    /// </summary>
    public IFormattedRegisterIndexer<long> Longs
    {
        get
        {
            bool use64BitMode = sizeof(TFloat) is sizeof(ulong) && _co0.FloatingPoint64BitMode;
            return use64BitMode
                ? RegisterFile.Longs
                : new PairedLongIndexer(RegisterFile.Regs);
        }
    }

    /// <summary>
    /// Gets or sets the value of a register on the coprocessor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public TFloat this[MipsFloatRegister reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <inheritdoc/>
    public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
    {
        var trap = _instructionServiceTable.Execute(instruction, out var fExecution);
        execution = fExecution.ChildExecution;

        if (trap is not MipsTrap.None)
            return trap;

        switch (fExecution.Format)
        {
            case FloatWriteback.Word:
                Words[(int)fExecution.Register] = (int)fExecution.Writeback;
                break;
            case FloatWriteback.Long:
                Longs[(int)fExecution.Register] = fExecution.Writeback;
                break;
            case FloatWriteback.Control:
                ControlRegisterFile[(int)fExecution.Register] = (uint)fExecution.Writeback;
                break;
        }

        return MipsTrap.None;
    }

    /// <inheritdoc/>
    public void CompileInstruction(ILGenerator il, MipsInstruction instruction, T pc)
    {
        _jitCompiler.CompileInstruction(il, instruction, pc);
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public readonly struct PairedDoubleIndexer(TFloat* regs) : IFormattedRegisterIndexer<double>
    {
        private readonly TFloat* _regs = regs;

        /// <inheritdoc/>
        public double this[int reg]
        {
            get
            {
                // Register must be even
                if (reg % 2 is not 0)
                    return double.NaN;

                // Convert the register pair to a double
                uint low = uint.CreateTruncating(_regs[reg]);
                uint high = uint.CreateTruncating(_regs[reg + 1]);
                ulong combined = ((ulong)high << 32) | low;
                return BitConverter.UInt64BitsToDouble(combined);
            }
            set
            {
                // Register must be even
                if (reg % 2 is not 0)
                    return;

                // Split the double into two uints
                var integer = BitConverter.DoubleToUInt64Bits(value);
                _regs[reg] = TFloat.CreateTruncating(integer & 0xFFFF_FFFF);
                _regs[reg + 1] = TFloat.CreateTruncating(integer >> 32);
            }
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public readonly struct PairedLongIndexer(TFloat* regs) : IFormattedRegisterIndexer<long>
    {
        private readonly TFloat* _regs = regs;

        /// <inheritdoc/>
        public long this[int reg]
        {
            get
            {
                // Register much be even
                if (reg % 2 is not 0)
                    return 0;

                // Convert the register pair to a double
                uint low = uint.CreateTruncating(_regs[reg]);
                uint high = uint.CreateTruncating(_regs[reg + 1]);
                return (long)(((ulong)high << 32) | low);
            }
            set
            {
                // Register much be even
                if (reg % 2 is not 0)
                    return;

                // Split the double into two uints
                _regs[reg] = TFloat.CreateTruncating(value & 0xFFFF_FFFF);
                _regs[reg + 1] = TFloat.CreateTruncating(value >> 32);
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        RegisterFile.Dispose();
        ControlRegisterFile.Dispose();
    }
}
