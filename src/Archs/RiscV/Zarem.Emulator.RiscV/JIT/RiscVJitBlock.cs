// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A record for a RISC-V JIT Block.
/// </summary>
public record RiscVJitBlock<T> : JitBlock<T, RiscVBlockDelegate<T>>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVJitBlock{T}"/> class.
    /// </summary>
    public RiscVJitBlock(RiscVBlockDelegate<T> @delegate, int size) : base(@delegate, size)
    {
    }
}
