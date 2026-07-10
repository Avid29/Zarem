// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A record for a JIT Block.
/// </summary>
public record JitBlock<T>(T Size)
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>;
