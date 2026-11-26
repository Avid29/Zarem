// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Machine.CoProcessors.Control;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Registers.FloatProcessor;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors.FloatingPoint;

/// <summary>
/// a class representing the floating-point coprocessor unit.
/// </summary>
public unsafe partial class FloatCoProcessor<T, TFloat> : IFloatCoProcessor<T>, IDisposable
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private readonly ControlCoProcessor<T> _co0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FloatCoProcessor{T, TFloat}"/> class.
    /// </summary>
    public FloatCoProcessor(MipsCpu<T> cpu)
    {
        _co0 = cpu.ControlCoProcessor;
        RegisterFile = new(32);
        ControlRegisterFile = new();

        Executor = cpu.Config.ExecutionMode switch
        {
            ExecutionMode.Interpret => new FloatInterpretedExecutor(cpu, this),
            ExecutionMode.JustInTime => new FloatJitExecutor(cpu, this),
            _ => ThrowHelper.ThrowArgumentException<ICoProcessor<T>.ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap>>(),
        };
    }

    /// <inheritdoc/>
    public ICoProcessor<T>.ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap> Executor { get; }

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
    public void CompileInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction instruction, T pc)
        => Executor.CompileInstruction(context, instruction, pc);

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
