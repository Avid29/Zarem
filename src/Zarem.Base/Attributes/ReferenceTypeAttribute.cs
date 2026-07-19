// Avishai Dernis 2026

using System;

namespace Zarem.Attributes;

/// <summary>
/// An attribute for an arch's reference type.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ReferenceTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceTypeAttribute"/> class.
    /// </summary>
    public ReferenceTypeAttribute(int bitCount)
    {
        BitCount = bitCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceTypeAttribute"/> class.
    /// </summary>
    public ReferenceTypeAttribute(string alias, int bitCount)
    {
        Alias = alias;
        BitCount = bitCount;
    }

    /// <summary>
    /// Gets the alias of the reference type.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// Gets the number of bits managed by the reference.
    /// </summary>
    public int BitCount { get; }

    /// <summary>
    /// Gets or sets the offset of value with this reference type.
    /// </summary>
    public int ShiftAmount { get; set; } = 0;
}
