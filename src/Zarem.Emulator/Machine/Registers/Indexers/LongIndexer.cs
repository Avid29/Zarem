// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.Machine.Registers.Indexers;

/// <summary>
/// An <see cref="IFormattedRegisterIndexer{long}"/>.
/// </summary>
public unsafe readonly struct LongIndexer<T>(T* regs) : IFormattedRegisterIndexer<long>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly T* _regs = regs;

    /// <inheritdoc/>
    public long this[int reg]
    {
        get => long.CreateTruncating(_regs[reg]);
        set => _regs[reg] = T.CreateTruncating(value);
    }
}
