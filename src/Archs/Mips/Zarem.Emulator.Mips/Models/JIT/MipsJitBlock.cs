// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Machine.JIT;

namespace Zarem.Emulator.Models.JIT
{
    /// <summary>
    /// A record for a MIPS JIT Block.
    /// </summary>
    public record MipsJitBlock<T>(MipsBlockDelegate<T> Delegate, int Size)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>;
}
