// Avishai Dernis 2026

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Zarem.Emulator.JIT;


/// <summary>
/// A cache manager for JIT blocks.
/// </summary>
public class JitBlockCache<T, TBlock>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TBlock : class
{
    private readonly Dictionary<T, TBlock> _cache = [];

    /// <summary>
    /// Attempts to retreive a cached block of JIT code.
    /// </summary>
    public bool TryGet(T pc, [NotNullWhen(true)] out TBlock? block)
        => _cache.TryGetValue(pc, out block);

    /// <summary>
    /// Stores a block of JIT code.
    /// </summary>
    public void Store(T pc, TBlock block)
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
