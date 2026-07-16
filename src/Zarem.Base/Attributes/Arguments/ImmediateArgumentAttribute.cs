// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Arguments;

/// <summary>
/// A <see cref="ArgumentAttribute"/> for immediate arguments.
/// </summary>
public class ImmediateArgumentAttribute : ArgumentAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImmediateArgumentAttribute"/> class.
    /// </summary>
    public ImmediateArgumentAttribute(int bitCount, bool signed, int shiftAmount = 0)
    {
        BitCount = bitCount;
        Signed = signed;
        ShiftAmount = shiftAmount;
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
    public int ShiftAmount { get; }
}
