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
            _sortedAddresess.Add(symbol.Address);
            _symbolMap.Add(symbol.Address, symbol);
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
}
