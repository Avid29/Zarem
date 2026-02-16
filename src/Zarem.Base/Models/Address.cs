// Adam Dernis 2024

using System;
using System.Diagnostics;
using Zarem.Models.Tables;

namespace Zarem.Models;

/// <summary>
/// A struct containing an address and the section it belongs to.
/// </summary>
[DebuggerDisplay("ToString")]
public struct Address
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> struct.
    /// </summary>
    public Address(Section? section, long offset)
    {
        Section = section;
        Offset = offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> struct.
    /// </summary>
    public Address(long offset)
    {
        Offset = offset;
    }

    /// <summary>
    /// Gets the section the address belongs to.
    /// </summary>
    public Section? Section { get; set; }

    /// <summary>
    /// Gets the offset of the address within the section.
    /// </summary>
    public long Offset { get; set; }

    /// <summary>
    /// Gets whether or not the value is relocatable.
    /// </summary>
    public readonly bool IsRelocatable => Section is not null;

    /// <summary>
    /// Attempts to add two <see cref="Address"/> structs.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="result">The resulting address.</param>
    /// <returns>Whether or not the addresses could be added.</returns>
    public static bool TryAdd(Address left, Address right, out Address result)
    {
        result = default;

        if (left.IsRelocatable && right.IsRelocatable)
            return false;

        var section = left.Section ?? right.Section;
        result = new Address(section, left.Offset + right.Offset);
        return true;
    }

    /// <summary>
    /// Attempts to subtract an <see cref="Address"/> from another.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="result">The resulting address.</param>
    /// <returns>Whether or not the addresses could be added.</returns>
    public static bool TrySubtract(Address left, Address right, out Address result)
    {
        result = default;

        if (left.Section == right.Section)
        {
            var value = left.Offset - right.Offset;
            result = new Address(null, value);
            return true;
        }

        if (right.IsRelocatable)
            return false;


        result = new Address(left.Section, left.Offset - right.Offset);
        return true;
    }

    /// <inheritdoc/>
    public static bool operator ==(Address left, Address right) => left.Offset == right.Offset && left.Section == right.Section;

    /// <inheritdoc/>
    public static bool operator !=(Address left, Address right) => left.Offset != right.Offset || left.Section != right.Section;

    /// <inheritdoc/>
    public static Address operator +(Address address, long offset) => new(address.Section, address.Offset + offset);
    
    /// <inheritdoc/>
    public static Address operator -(Address address, long offset) => new(address.Section, address.Offset - offset);

    /// <inheritdoc/>
    public override readonly string ToString() => $"{Section?.Name}+0x{Offset:X}";

    /// <inheritdoc/>
    public readonly override bool Equals(object? obj) => base.Equals(obj);

    /// <inheritdoc/>
    public readonly override int GetHashCode() => HashCode.Combine(Offset, Section);
}
