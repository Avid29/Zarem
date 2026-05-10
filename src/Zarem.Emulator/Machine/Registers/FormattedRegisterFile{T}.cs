// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.Registers.Indexers;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// A register file with indexers for different formats.
/// </summary>
/// <param name="count"></param>
public unsafe class FormattedRegisterFile<T>(int count) : RegisterFile<T>(count), IFormattedRegisterFile
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    public IFormattedRegisterIndexer<Half> Halves => new HalfIndexer<T>(Regs);

    /// <inheritdoc/>
    public IFormattedRegisterIndexer<float> Singles => new SingleIndexer<T>(Regs);

    /// <inheritdoc/>
    public IFormattedRegisterIndexer<double> Doubles => new DoubleIndexer<T>(Regs);

    /// <inheritdoc/>
    public IFormattedRegisterIndexer<int> Words => new WordIndexer<T>(Regs);

    /// <inheritdoc/>
    public IFormattedRegisterIndexer<long> Longs => new LongIndexer<T>(Regs);

}
