// Avishai Dernis 2026

namespace Zarem.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a <see cref="FloatProcessor{T}"/> without a concrete type.
/// </summary>
public interface IFloatProcessor
{
    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="float"/>.
    /// </summary>
    IFloatRegisterIndexer<float> Singles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="double"/>.
    /// </summary>
    IFloatRegisterIndexer<double> Doubles { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as an <see cref="int"/>.
    /// </summary>
    IFloatRegisterIndexer<int> Words { get; }

    /// <summary>
    /// Gets an indexer for accessing the registers on the coprocessor as a <see cref="long"/>.
    /// </summary>
    IFloatRegisterIndexer<long> Longs { get; }
}
