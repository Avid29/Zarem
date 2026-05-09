// Avishai Dernis 2026

using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.Machine.CoProcessors;

namespace Zarem.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a <see cref="FloatProcessor{T}"/> without a concrete type.
/// </summary>
public interface IFloatProcessor
{
    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    IFormattedRegisterIndexer<float> Singles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="double"/>.
    /// </summary>
    IFormattedRegisterIndexer<double> Doubles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as an <see cref="int"/>.
    /// </summary>
    IFormattedRegisterIndexer<int> Words { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="long"/>.
    /// </summary>
    IFormattedRegisterIndexer<long> Longs { get; }
}
