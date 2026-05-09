// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.Machine.Registers.Indexers;

/// <summary>
/// An <see cref="IFormattedRegisterIndexer{int}"/>.
/// </summary>
public unsafe readonly struct WordIndexer<T>(T* regs) : IFormattedRegisterIndexer<int>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly T* _regs = regs;

    /// <inheritdoc/>
    public int this[int reg]
    {
        get => int.CreateTruncating(_regs[reg]);
        set => _regs[reg] = T.CreateTruncating(value);
    }
}
