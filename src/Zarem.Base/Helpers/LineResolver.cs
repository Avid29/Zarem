// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Helpers;

/// <summary>
/// A class for identifying the address of a given line.
/// </summary>
public class LineResolver
{
    private readonly Dictionary<(string?, ulong), Address> _lookup = [];
    private readonly SortedList<ulong, SourceRange> _sourceLookup = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LineResolver"/> class.
    /// </summary>
    public LineResolver(IEnumerable<LineEntry> lines)
    {
        foreach (var line in lines)
        {
            var key = (line.Location.Start.File, (ulong)line.Location.Start.Line);
            if (!_lookup.ContainsKey(key))
            {
                _lookup[key] = line.Address;
            }

            if (line.Address.VirtualAddress.HasValue)
            {
                _sourceLookup[line.Address.VirtualAddress.Value] = line.Location;
            }
        }
    }

    /// <summary>
    /// Gets the address of a given line in a file.
    /// </summary>
    /// <param name="filePath">The file the line belongs to.</param>
    /// <param name="lineNumber">The line number in the file.</param>
    /// <returns></returns>
    public Address? GetAddress(string filePath, ulong lineNumber)
        => _lookup.TryGetValue((filePath, lineNumber), out var address) ? address : null;

    /// <summary>
    /// Gets the file and line number given a virtual address.
    /// </summary>
    /// <param name="address">The virtual address</param>
    public SourceRange? GetSourceLocation(ulong address)
    {
        if (_sourceLookup.Count == 0)
            return null;

        // Binary search for the index
        int index = BinarySearchKeys(address);

        // Address is before our first registered point
        if (index == -1)
            return null; 

        return _sourceLookup.Values[index];
    }

    private int BinarySearchKeys(ulong key)
    {
        int low = 0;
        int high = _sourceLookup.Count - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (_sourceLookup.Keys[mid] == key) return mid;
            if (_sourceLookup.Keys[mid] < key) low = mid + 1;
            else high = mid - 1;
        }
        return high; // Returns the index of the greatest key less than the search key
    }
}
