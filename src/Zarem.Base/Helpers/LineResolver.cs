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
    private readonly Dictionary<string, SortedList<ulong, Address>> _lineLookup = [];
    private readonly SortedList<ulong, SourceRange> _sourceLookup = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LineResolver"/> class.
    /// </summary>
    public LineResolver(IEnumerable<LineEntry> lines)
    {
        foreach (var line in lines)
        {
            var filePath = line.Location.Start.File ?? string.Empty;
            var lineNum = line.Location.Start.Line;

            if (line.Address.VirtualAddress.HasValue)
            {
                _sourceLookup[line.Address.VirtualAddress.Value] = line.Location;
            }

            if (!_lineLookup.TryGetValue(filePath, out var list))
                _lineLookup[filePath] = list = [];

            list.TryAdd((ulong)lineNum, line.Address);
        }
    }

    /// <summary>
    /// Gets the address of a given line in a file.
    /// </summary>
    /// <param name="filePath">The file the line belongs to.</param>
    /// <param name="lineNumber">The line number in the file.</param>
    /// <returns></returns>
    public Address? GetAddress(string filePath, ulong lineNumber)
    {
        if (!_lineLookup.TryGetValue(filePath, out var list))
            return null;

        return BinarySearch(list, lineNumber, preferLower: false);
    }

    /// <summary>
    /// Gets the file and line number given a virtual address.
    /// </summary>
    /// <param name="address">The virtual address</param>
    public SourceRange? GetSourceLocation(ulong address)
        => BinarySearch(_sourceLookup, address, preferLower: true);

    private static T? BinarySearch<T>(SortedList<ulong, T> list, ulong key, bool preferLower = false)
    {
        if (list.Count is 0)
            return default;

        int index = BinarySearchKeys(list.Keys, key, preferLower);

        if (index is < 0 || index >= list.Count)
            return default;

        return list.Values[index];

    }

    private static int BinarySearchKeys(IList<ulong> list, ulong key, bool preferLower)
    {
        int low = 0;
        int high = list.Count - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (list[mid] == key) return mid;
            if (list[mid] < key) low = mid + 1;
            else high = mid - 1;
        }

        return preferLower
            ? high // Returns the index of the greatest key less than or equal to the search key
            : low; // Returns the index of the smallest key greater than or equal to the search key
    }
}
