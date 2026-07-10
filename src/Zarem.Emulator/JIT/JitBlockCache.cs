// Avishai Dernis 2026

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A cache manager for JIT blocks.
/// </summary>
public class JitBlockCache<T, TBlock>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TBlock : JitBlock<T>
{
    private readonly Dictionary<T, TBlock> _blockCache = [];
    private readonly Dictionary<T, HashSet<T>> _pageCache = [];
    private readonly T _pageSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="JitBlockCache{T, TBlock}"/> class.
    /// </summary>
    public JitBlockCache(int pageSize = 4096) : this(T.CreateChecked(pageSize))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JitBlockCache{T, TBlock}"/> class.
    /// </summary>
    public JitBlockCache(T pageSize)
    {
        _pageSize = pageSize;
    }

    /// <summary>
    /// Attempts to retreive a cached block of JIT code.
    /// </summary>
    public bool TryGet(T pc, [NotNullWhen(true)] out TBlock? block)
        => _blockCache.TryGetValue(pc, out block);

    /// <summary>
    /// Stores a block of JIT code.
    /// </summary>
    public void Store(T pc, TBlock block)
    {
        // Invalidate the block if it already exists in the cache.
        if (TryGet(pc, out var _))
            InvalidateBlock(pc);

        // Store in the block cache.
        _blockCache[pc] = block;

        // Track the block in the page cache
        // This allows us to invalidate all blocks in a page when the page is invalidated.
        var (startPage, endPage) = GetBlockPageRange(pc, block);

        // Add the block to all pages it spans
        // NOTE: Use the CollectionsMarshal to avoid looking up the page twice in the dictionary.
        for (var page = startPage; page <= endPage; page++)
        {
            ref var pageBlocks = ref CollectionsMarshal.GetValueRefOrAddDefault(_pageCache, page, out bool exists);
            if (!exists || pageBlocks is null)
                pageBlocks = [];

            pageBlocks.Add(pc);
        }
    }

    /// <summary>
    /// Invalidates a page of JIT code blocks.
    /// </summary>
    public void InvalidatePage(T pc)
    {
        var page = pc / _pageSize;
        _pageCache.Remove(page, out var blocks);
        if (blocks is null)
            return;

        foreach (var blockPc in blocks)
            InvalidateBlock(blockPc);
    }

    /// <summary>
    /// Invalidates a block of JIT code.
    /// </summary>
    public void InvalidateBlock(T pc)
    {
        // Remove from block cache
        if (!_blockCache.Remove(pc, out var block))
            return;

        // Stop tracking the block in page cache
        var (startPage, endPage) = GetBlockPageRange(pc, block);
        for (var page = startPage; page <= endPage; page ++)
        {
            if (!_pageCache.TryGetValue(page, out var blocks))
                continue;

            blocks.Remove(pc);

            // If the page has no more blocks, remove it from the page cache.
            if (blocks.Count is 0)
            {
                _pageCache.Remove(page);
            }
        }    
    }

    /// <summary>
    /// Clears the JIT code cache.
    /// </summary>
    public void Clear()
    {
        _blockCache.Clear();
        _pageCache.Clear();
    }

    private (T startPage, T endPage) GetBlockPageRange(T pc, TBlock block)
    {
        var startPage = pc / _pageSize;
        var endPage = (pc + T.CreateChecked(block.Size) - T.One) / _pageSize;
        return (startPage, endPage);
    }
}
