// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Arguments;

/// <summary>
/// A <see cref="ArgumentAttribute"/> for immediate arguments.
/// </summary>
public class ImmediateArgumentAttribute<TRef> : ArgumentAttribute
    where TRef : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImmediateArgumentAttribute{TRef}"/> class.
    /// </summary>
    public ImmediateArgumentAttribute(string alias, int bitCount, bool signed) : base(alias)
    {
        BitCount = bitCount;
        Signed = signed;
    }

    /// <summary>
    /// Gets the bit count of the argument's immediate.
    /// </summary>
    public int BitCount { get; }

    /// <summary>
    /// Gets whether or not the argument's immediate is signed.
    /// </summary>
    public bool Signed { get; }

    /// <summary>
    /// Gets the shift amount of the argument's immediate.
    /// </summary>
    public int ShiftAmount { get; set; } = 0;

    /// <summary>
    /// Gets the default relocation type for the 
    /// </summary>
    public TRef DefaultRelocation { get; set; } = default;
}
