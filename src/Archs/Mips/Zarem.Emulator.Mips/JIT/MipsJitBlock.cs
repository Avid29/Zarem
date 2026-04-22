// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A record for a MIPS JIT Block.
/// </summary>
public record MipsJitBlock<T> : JitBlock<T, MipsBlockDelegate<T>>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitBlock{T}"/> class.
    /// </summary>
    public MipsJitBlock(MipsBlockDelegate<T> @delegate, int size) : base(@delegate, size)
    {
    }
}
