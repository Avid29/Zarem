// Avishai Dernis 2026

using System;
using System.Numerics;

namespace Zarem.Emulator.Machine.Registers.Indexers;

/// <summary>
/// An <see cref="IFormattedRegisterIndexer{float}"/>.
/// </summary>
public unsafe readonly struct SingleIndexer<T>(T* regs) : IFormattedRegisterIndexer<float>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly T* _regs = regs;

    /// <inheritdoc/>
    public float this[int reg]
    {
        get => BitConverter.UInt32BitsToSingle(uint.CreateTruncating(_regs[reg]));
        set => _regs[reg] = T.CreateTruncating(BitConverter.SingleToUInt32Bits(value));
    }
}
