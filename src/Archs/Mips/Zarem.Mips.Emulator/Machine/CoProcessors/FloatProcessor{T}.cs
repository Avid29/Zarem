// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.InteropServices.Swift;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Machine.Registers.Indexers;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// a class representing the floating-point coprocessor unit.
/// </summary>
public unsafe class FloatProcessor<T> : IFloatProcessor
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FloatProcessor{T}"/> class.
    /// </summary>
    public FloatProcessor()
    {
        RegisterFile = new(32);
    }

    internal RegisterFile<T> RegisterFile { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    public IFormattedRegisterIndexer<float> Singles => new SingleIndexer<T>(RegisterFile.Regs);

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="double"/>.
    /// </summary>
    public IFormattedRegisterIndexer<double> Doubles
    {
        get
        {
            return sizeof(T) switch
            {
                sizeof(uint) => new PairedDoubleIndexer(RegisterFile.Regs),
                sizeof(ulong) => new DoubleIndexer<T>(RegisterFile.Regs),
                _ => ThrowHelper.ThrowNotSupportedException<IFormattedRegisterIndexer<double>>(),
            };
        }
    }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as an <see cref="int"/>.
    /// </summary>
    public IFormattedRegisterIndexer<int> Words => new WordIndexer<T>(RegisterFile.Regs);

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="long"/>.
    /// </summary>
    public IFormattedRegisterIndexer<long> Longs
    {
        get
        {
            return sizeof(T) switch
            {
                sizeof(uint) => new PairedLongIndexer(RegisterFile.Regs),
                sizeof(ulong) => new LongIndexer<T>(RegisterFile.Regs),
                _ => ThrowHelper.ThrowNotSupportedException<IFormattedRegisterIndexer<long>>(),
            };
        }
    }

    /// <summary>
    /// Gets or sets the value of a register on the coprocessor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public T this[MipsFloatRegister reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public readonly struct PairedDoubleIndexer(T* regs) : IFormattedRegisterIndexer<double>
    {
        private readonly T* _regs = regs;

        /// <inheritdoc/>
        public double this[int reg]
        {
            get
            {
                // Register much be even
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
                // Register much be even
                if (reg % 2 is not 0)
                    return;

                // Split the double into two uints
                var integer = BitConverter.DoubleToUInt64Bits(value);
                _regs[reg] = T.CreateTruncating(integer & 0xFFFF_FFFF);
                _regs[reg + 1] = T.CreateTruncating(integer >> 32);
            }
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public readonly struct PairedLongIndexer(T* regs) : IFormattedRegisterIndexer<long>
    {
        private readonly T* _regs = regs;

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
                _regs[reg] = T.CreateTruncating(value & 0xFFFF_FFFF);
                _regs[reg + 1] = T.CreateTruncating(value >> 32);
            }
        }
    }
}
