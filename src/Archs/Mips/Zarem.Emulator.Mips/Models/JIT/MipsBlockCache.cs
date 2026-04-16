// Avishai Dernis 2026

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Zarem.Emulator.Machine.JIT;

namespace Zarem.Emulator.Models.JIT;

/// <summary>
/// A cache manager for MIPS JIT blocks.
/// </summary>
public class MipsBlockCache<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly Dictionary<T, MipsBlockDelegate<T>> _cache = [];

    /// <summary>
    /// Attempts to retreive a cached block of JIT code.
    /// </summary>
    public bool TryGet(T pc, [NotNullWhen(true)] out MipsBlockDelegate<T>? block)
        => _cache.TryGetValue(pc, out block);

    /// <summary>
    /// Stores a block of JIT code.
    /// </summary>
    public void Store(T pc, MipsBlockDelegate<T> block)
        => _cache[pc] = block;

    /// <summary>
    /// Invalidates a block of JIT code.
    /// </summary>
    public void Invalidate(T pc) => _cache.Remove(pc);

    /// <summary>
    /// Clears the JIT code cache.
    /// </summary>
    public void Clear() => _cache.Clear();
}
