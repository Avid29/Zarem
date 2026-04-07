// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Helpers;

/// <summary>
/// A class for identifying the nearest symbol to an address.
/// </summary>
public class SymbolResolver
{
    private readonly List<Address> _sortedAddresess = [];
    private readonly Dictionary<Address, Symbol> _symbolMap = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolResolver"/> class.
    /// </summary>
    /// <param name="symbols"></param>
    public SymbolResolver(IEnumerable<Symbol> symbols) => Rebuild(symbols);

    /// <summary>
    /// Rebuilds the resolver with a new set of symbols.
    /// </summary>
    /// <param name="symbols">The new collection of symbols.</param>
    public void Rebuild(IEnumerable<Symbol> symbols)
    {
        _symbolMap.Clear();
        _sortedAddresess.Clear();

        // If we know the collection size, we can prevent multiple internal 
        // array resizes by ensuring capacity upfront.
        if (symbols is ICollection<Symbol> collection)
        {
            _sortedAddresess.Capacity = Math.Max(_sortedAddresess.Capacity, collection.Count);
        }

        foreach (Symbol symbol in symbols)
        {
            // NOTE: A single address can have multiple symbols.
            // Currently we just take the first, but in the future we might like to track 
            // every matching symbol to their address
            _sortedAddresess.Add(symbol.Address);
            _symbolMap.TryAdd(symbol.Address, symbol);
        }

        _sortedAddresess.Sort();
    }

    /// <summary>
    /// Finds the nearest preceding symbol to the address.
    /// </summary>
    /// <param name="target">The address to identify a symbol for.</param>
    /// <param name="proceeding">The nearest proceeding symbol to the address</param>
    /// <param name="ensureSection">Whether or not to exclusively return symbols from the same section as the address.</param>
    /// <returns>The nearest preceding symbol to the address.</returns>
    public Symbol? FindNearest(Address target, out Symbol? proceeding, bool ensureSection = true)
    {
        proceeding = null;
        int index = _sortedAddresess.BinarySearch(target);

        int precedingIndex;
        int followingIndex;

        if (index >= 0)
        {
            // Exact match found
            precedingIndex = index;
            followingIndex = index + 1;
        }
        else
        {
            // No exact match. ~index is the first element greater than target
            followingIndex = ~index;
            precedingIndex = followingIndex - 1;
        }

        // Resolve the proceeding following)symbol
        if (followingIndex < _sortedAddresess.Count)
        {
            var nextAddr = _sortedAddresess[followingIndex];
            if (!ensureSection || nextAddr.Section == target.Section)
            {
                proceeding = _symbolMap[nextAddr];
            }
        }

        // Resolve the preceding symbol
        if (precedingIndex >= 0)
        {
            var prevAddr = _sortedAddresess[precedingIndex];
            if (!ensureSection || prevAddr.Section == target.Section)
            {
                return _symbolMap[prevAddr];
            }
        }

        return null;
    }
    
    /// <summary>
    /// Finds the nearest preceding symbol to a raw virtual address.
    /// </summary>
    /// <param name="virtualAddress">The raw memory address to look up.</param>
    /// <param name="proceeding">The nearest proceeding symbol to the address.</param>
    /// <returns>The nearest preceding symbol to the address.</returns>
    public Symbol? FindNearest(ulong virtualAddress, out Symbol? proceeding)
    {
        proceeding = null;

        // We need to find where this ulong would fit in our sorted list.
        // Since Address.CompareTo uses Section.VirtualAddress for comparisons 
        // across sections, we can binary search using a 'pseudo-address'.

        int index = BinarySearchByVirtualAddress(virtualAddress);

        int precedingIndex;
        int followingIndex;

        if (index >= 0)
        {
            // Exact virtual address match found
            precedingIndex = index;
            followingIndex = index + 1;
        }
        else
        {
            // No exact match. ~index is the first element with a VA greater than target
            followingIndex = ~index;
            precedingIndex = followingIndex - 1;
        }

        if (followingIndex < _sortedAddresess.Count)
        {
            proceeding = _symbolMap[_sortedAddresess[followingIndex]];
        }

        if (precedingIndex >= 0)
        {
            return _symbolMap[_sortedAddresess[precedingIndex]];
        }

        return null;
    }

    private int BinarySearchByVirtualAddress(ulong targetVa)
    {
        int low = 0;
        int high = _sortedAddresess.Count - 1;

        while (low <= high)
        {
            int mid = low + ((high - low) >> 1);
            ulong? midVa = _sortedAddresess[mid].VirtualAddress;

            // Handle cases where an address might not have a virtual address 
            // (Section is null). We treat null VAs as smaller than any real VA.
            if (!midVa.HasValue)
            {
                low = mid + 1;
                continue;
            }

            int compare = midVa.Value.CompareTo(targetVa);

            if (compare == 0) return mid;
            if (compare < 0) low = mid + 1;
            else high = mid - 1;
        }

        return ~low;
    }
}
