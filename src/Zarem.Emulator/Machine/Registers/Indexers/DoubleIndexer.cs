// Avishai Dernis 2026

using System;
using System.Numerics;

namespace Zarem.Emulator.Machine.Registers.Indexers;

/// <summary>
/// An <see cref="IFormattedRegisterIndexer{double}"/>.
/// </summary>
public unsafe readonly struct DoubleIndexer<T>(T* regs) : IFormattedRegisterIndexer<double>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly T* _regs = regs;

    /// <inheritdoc/>
    public double this[int reg]
    {
        get => BitConverter.UInt64BitsToDouble(ulong.CreateTruncating(_regs[reg]));
        set => _regs[reg] = T.CreateTruncating(BitConverter.DoubleToUInt64Bits(value));
    }
}
