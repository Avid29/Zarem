// Avishai Dernis 2026

using Zarem.Emulator.Machine.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a <see cref="FloatCoProcessor{T, TFloat}"/> without a concrete type.
/// </summary>
public interface IFloatCoProcessor : ICoProcessor
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
