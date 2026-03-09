// Avishai Dernis 2026

using System;
using System.Runtime.InteropServices;
using Zarem.Emulator.Machine.CPU.Registers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.CPU.CoProcessors;

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
        Doubles = new(this);
    }

    internal RegisterFile RegisterFile { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    public SingleIndexer Singles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="double"/>.
    /// </summary>
    public DoubleIndexer Doubles { get; }

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
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public readonly struct SingleIndexer(FloatProcessor parent)
    {
        private readonly FloatProcessor _parent = parent;

        /// <summary>
        /// Gets or sets the value of a register on the coprocessor as a <see cref="float"/>.
        /// </summary>
        /// <param name="reg">The register to get or set.</param>
        /// <returns>The value of the register.</returns>
        public float this[FloatRegister reg]
        {
            get => BitConverter.Int32BitsToSingle((int)_parent[reg]);
            set => _parent[reg] = BitConverter.SingleToUInt32Bits(value);
        }
    }

    /// <summary>
    /// An wrapper to access floating-point register pairs as doubles.
    /// </summary>
    public readonly struct DoubleIndexer(FloatProcessor parent)
    {
        private readonly FloatProcessor _parent = parent;

        /// <summary>
        /// Gets or sets the value of a register on the coprocessor as a <see cref="double"/>.
        /// </summary>
        /// <param name="reg">The register to get or set.</param>
        /// <returns>The value of the register.</returns>
        public double this[FloatRegister reg]
        {
            get
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return double.NaN;

                // Convert the register pair to a double
                ReadOnlySpan<uint> parts = [_parent.RegisterFile[reg], _parent.RegisterFile[reg + 1]];
                return MemoryMarshal.Cast<uint, double>(parts)[0];
            }
            set
            {
                // Register much be even
                if ((int)reg % 2 is not 0)
                    return;

                // Split the double into two uints
                Span<uint> parts = stackalloc uint[2];
                MemoryMarshal.Cast<uint, double>(parts)[0] = value;

                // Store the parts
                _parent.RegisterFile[reg] = parts[0];
                _parent.RegisterFile[reg + 1] = parts[1];
            }
        }
    }
}
