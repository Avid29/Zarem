// Avishai Dernis 2026

using System;
using System.Numerics;

namespace Zarem.Emulator.Machine.Registers.Indexers;

/// <summary>
/// An <see cref="IFormattedRegisterIndexer{float}"/>.
/// </summary>
public unsafe readonly struct HalfIndexer<T>(T* regs) : IFormattedRegisterIndexer<Half>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly T* _regs = regs;

    /// <inheritdoc/>
    public Half this[int reg]
    {
        get => BitConverter.UInt16BitsToHalf(ushort.CreateTruncating(_regs[reg]));
        set => _regs[reg] = T.CreateTruncating(BitConverter.HalfToUInt16Bits(value));
    }
}
