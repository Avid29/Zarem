// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Zarem.Emulator.Machine.Registers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.CoProcessors;

/// <summary>
/// a class representing the floating-point coprocessor unit.
/// </summary>
public class FloatProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FloatProcessor"/> class.
    /// </summary>
    public FloatProcessor()
    {
        RegisterFile = new();
        Singles = new SingleIndexer(this);
        Doubles = new DoubleIndexer(this);
        Words = new WordIndexer(this);
        Longs = new LongIndexer(this);
    }

    internal RegisterFile RegisterFile { get; }

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
    public uint this[FloatRegister reg]
    {
        get => RegisterFile[reg];
        set => RegisterFile[reg] = value;
    }

    /// <summary>
    /// An interface for indexing the FPU registers with different formats.
    /// </summary>
    /// <typeparam name="T">The indexer's format.</typeparam>
    public interface IFloatRegisterIndexer<T>
        where T : INumber<T>
    {
        /// <summary>
        /// Gets or sets the value of a register on the coprocessor as a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="reg">The register to get or set.</param>
        /// <returns>The value of the register.</returns>
        T this[FloatRegister reg] { get; set; }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class SingleIndexer(FloatProcessor parent) : IFloatRegisterIndexer<float>
    {
        private readonly FloatProcessor _parent = parent;

        /// <inheritdoc/>
        public float this[FloatRegister reg]
        {
            get => BitConverter.UInt32BitsToSingle(_parent[reg]);
            set => _parent[reg] = BitConverter.SingleToUInt32Bits(value);
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class DoubleIndexer(FloatProcessor parent) : IFloatRegisterIndexer<double>
    {
        private readonly FloatProcessor _parent = parent;

        /// <inheritdoc/>
        public double this[FloatRegister reg]
        {
            get
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return double.NaN;

                // Convert the register pair to a double
                uint low = _parent.RegisterFile[reg];
                uint high = _parent.RegisterFile[reg + 1];
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
                _parent.RegisterFile[reg] = (uint)(integer & 0xFFFF_FFFF);
                _parent.RegisterFile[reg + 1] = (uint)(integer >> 32);
            }
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class WordIndexer(FloatProcessor parent) : IFloatRegisterIndexer<int>
    {
        private readonly FloatProcessor _parent = parent;

        /// <inheritdoc/>
        public int this[FloatRegister reg]
        {
            get => (int)_parent[reg];
            set => _parent[reg] = (uint)value;
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public class LongIndexer(FloatProcessor parent) : IFloatRegisterIndexer<long>
    {
        private readonly FloatProcessor _parent = parent;

        /// <inheritdoc/>
        public long this[FloatRegister reg]
        {
            get
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return 0;

                // Convert the register pair to a double
                uint low = _parent.RegisterFile[reg];
                uint high = _parent.RegisterFile[reg + 1];
                return (long)((ulong)high << 32) | low;
            }
            set
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return;

                // Split the double into two uints
                _parent.RegisterFile[reg] = (uint)(value & 0xFFFF_FFFF);
                _parent.RegisterFile[reg + 1] = (uint)(value >> 32);
            }
        }
    }
}
