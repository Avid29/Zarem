// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.CoProcessors;

/// <summary>
/// a class representing the floating-point coprocessor unit.
/// </summary>
public unsafe class FloatProcessor<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FloatProcessor{T}"/> class.
    /// </summary>
    public FloatProcessor()
    {
        RegisterFile = new(32);

        Singles = new SingleIndexer(this);
        Words = new WordIndexer(this);
        
        if (sizeof(T) == sizeof(uint))
        {
            Doubles = new PairedDoubleIndexer(this);
            Longs = new PairedLongIndexer(this);
        }
        else
        {
            Doubles = new DoubleIndexer(this);
            Longs = new LongIndexer(this);
        }
    }

    internal RegisterFile<T> RegisterFile { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    public IFloatRegisterIndexer<float> Singles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="double"/>.
    /// </summary>
    public IFloatRegisterIndexer<double> Doubles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as an <see cref="int"/>.
    /// </summary>
    public IFloatRegisterIndexer<int> Words { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="long"/>.
    /// </summary>
    public IFloatRegisterIndexer<long> Longs { get; }

    /// <summary>
    /// Gets or sets the value of a register on the coprocessor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public T this[FloatRegister reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <summary>
    /// An interface for indexing the FPU registers with different formats.
    /// </summary>
    /// <typeparam name="T2">The indexer's format.</typeparam>
    public interface IFloatRegisterIndexer<T2>
        where T2 : INumber<T2>
    {
        /// <summary>
        /// Gets or sets the value of a register on the coprocessor as a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="reg">The register to get or set.</param>
        /// <returns>The value of the register.</returns>
        T2 this[FloatRegister reg] { get; set; }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class SingleIndexer(FloatProcessor<T> parent) : IFloatRegisterIndexer<float>
    {
        private readonly FloatProcessor<T> _parent = parent;

        /// <inheritdoc/>
        public float this[FloatRegister reg]
        {
            get => BitConverter.UInt32BitsToSingle(uint.CreateTruncating(_parent[reg]));
            set => _parent[reg] = T.CreateTruncating(BitConverter.SingleToUInt32Bits(value));
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class WordIndexer(FloatProcessor<T> parent) : IFloatRegisterIndexer<int>
    {
        private readonly FloatProcessor<T> _parent = parent;

        /// <inheritdoc/>
        public int this[FloatRegister reg]
        {
            get => int.CreateTruncating(_parent[reg]);
            set => _parent[reg] = T.CreateTruncating(value);
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class DoubleIndexer(FloatProcessor<T> parent) : IFloatRegisterIndexer<double>
    {
        private readonly FloatProcessor<T> _parent = parent;

        /// <inheritdoc/>
        public double this[FloatRegister reg]
        {
            get => BitConverter.UInt64BitsToDouble(ulong.CreateTruncating(_parent[reg]));
            set => _parent[reg] = T.CreateTruncating(BitConverter.DoubleToUInt64Bits(value));
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class LongIndexer(FloatProcessor<T> parent) : IFloatRegisterIndexer<long>
    {
        private readonly FloatProcessor<T> _parent = parent;

        /// <inheritdoc/>
        public long this[FloatRegister reg]
        {
            get => long.CreateTruncating(_parent[reg]);
            set => _parent[reg] = T.CreateTruncating(value);
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class PairedDoubleIndexer(FloatProcessor<T> parent) : IFloatRegisterIndexer<double>
    {
        private readonly FloatProcessor<T> _parent = parent;

        /// <inheritdoc/>
        public double this[FloatRegister reg]
        {
            get
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return double.NaN;

                // Convert the register pair to a double
                uint low = uint.CreateTruncating(_parent.RegisterFile[(int)reg]);
                uint high = uint.CreateTruncating(_parent.RegisterFile[(int)(reg + 1)]);
                ulong combined = ((ulong)high << 32) | low;
                return BitConverter.UInt64BitsToDouble(combined);
            }
            set
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return;

                // Split the double into two uints
                var integer = BitConverter.DoubleToUInt64Bits(value);
                _parent.RegisterFile[(int)reg] = T.CreateTruncating(integer & 0xFFFF_FFFF);
                _parent.RegisterFile[(int)(reg + 1)] = T.CreateTruncating(integer >> 32);
            }
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class PairedLongIndexer(FloatProcessor<T> parent) : IFloatRegisterIndexer<long>
    {
        private readonly FloatProcessor<T> _parent = parent;

        /// <inheritdoc/>
        public long this[FloatRegister reg]
        {
            get
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return 0;

                // Convert the register pair to a double
                uint low = uint.CreateTruncating(_parent.RegisterFile[(int)reg]);
                uint high = uint.CreateTruncating(_parent.RegisterFile[(int)(reg + 1)]);
                return (long)(((ulong)high << 32) | low);
            }
            set
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return;

                // Split the double into two uints
                _parent.RegisterFile[(int)reg] = T.CreateTruncating(value & 0xFFFF_FFFF);
                _parent.RegisterFile[(int)(reg + 1)] = T.CreateTruncating(value >> 32);
            }
        }
    }
}
