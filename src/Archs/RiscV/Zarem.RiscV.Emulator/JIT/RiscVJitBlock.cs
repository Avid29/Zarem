// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.JIT;

namespace Zarem.RiscV.Emulator.JIT;

/// <summary>
/// A record for a RISC-V JIT Block.
/// </summary>
public record RiscVJitBlock<T, TFloat> : JitBlock<T, RiscVBlockDelegate<T, TFloat>>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVJitBlock{T, TFloat}"/> class.
    /// </summary>
    public RiscVJitBlock(RiscVBlockDelegate<T, TFloat> @delegate, T size) : base(@delegate, size)
    {
    }
}
