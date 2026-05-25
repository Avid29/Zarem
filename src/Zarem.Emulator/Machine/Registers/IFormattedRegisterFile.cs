// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// An interface for a register file with different indexable formats.
/// </summary>
public interface IFormattedRegisterFile : IRegisterFile
{
    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="Half"/>.
    /// </summary>
    IFormattedRegisterIndexer<Half> Halves { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    IFormattedRegisterIndexer<float> Singles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    IFormattedRegisterIndexer<double> Doubles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="int"/>.
    /// </summary>
    IFormattedRegisterIndexer<int> Words { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="long"/>.
    /// </summary>
    IFormattedRegisterIndexer<long> Longs { get; }
}
