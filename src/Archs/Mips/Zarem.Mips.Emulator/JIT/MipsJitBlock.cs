// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.InteropServices;
using Zarem.Emulator.JIT;

namespace Zarem.Mips.Emulator.JIT;

/// <summary>
/// A record for a MIPS JIT Block.
/// </summary>
public record MipsJitBlock<T, TFloat> : JitBlock<T, MipsBlockDelegate<T, TFloat>>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitBlock{T, TFloat}"/> class.
    /// </summary>
    public MipsJitBlock(MipsBlockDelegate<T, TFloat> @delegate, T size) : base(@delegate, size)
    {
    }
}
