// Avishai Dernis 2026

using System;

namespace Zarem.Models.Tables;

/// <summary>
/// A record pairing an address to a line of assembly code.
/// </summary>
public record LineEntry(Address Address, SourceLocation Location) : IComparable<LineEntry>
{
    /// <inheritdoc/>
    public int CompareTo(LineEntry? other) => Address.CompareTo(other?.Address ?? default);
}
