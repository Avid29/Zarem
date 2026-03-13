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
    private readonly Dictionary<ulong, (string?, ulong)> _sourceLookup = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LineResolver"/> class.
    /// </summary>
    public LineResolver(IEnumerable<LineEntry> lines)
    {
        foreach (var line in lines)
        {
            var key = (line.Location.File, (ulong)line.Location.Line);
            if (!_lookup.ContainsKey(key))
            {
                _lookup[key] = line.Address;

                if (line.Address.VirtualAddress.HasValue)
                {
                    _sourceLookup[line.Address.VirtualAddress.Value] = key;
                }
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
        => _lookup.TryGetValue((filePath, lineNumber + 1), out var address) ? address : null;

    /// <summary>
    /// Gets the file and line number given a virtual address.
    /// </summary>
    /// <param name="address">The virtual address</param>
    public (string?, ulong)? GetSourceLine(ulong address) => _sourceLookup.TryGetValue(address, out var sourceLine) ? sourceLine : null;
}
