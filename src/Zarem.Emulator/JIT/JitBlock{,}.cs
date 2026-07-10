// Avishai Dernis 2026

using System;
using System.Numerics;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A record for a JIT Block.
/// </summary>
public record JitBlock<T, TDelegate>(TDelegate Delegate, T Size) : JitBlock<T>(Size)
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TDelegate : Delegate;
