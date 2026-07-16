// Avishai Dernis 2026

namespace Zarem.Attributes;

/// <summary>
/// A <see cref="AssemblerArgumentAttribute"/> for immediate arguments.
/// </summary>
public class ImmediateArgumentAttribute : AssemblerArgumentAttribute
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

    /// <inheritdoc/>
    public int BitCount { get; }

    /// <inheritdoc/>
    public bool Signed { get; }

    /// <inheritdoc/>
    public int ShiftAmount { get; }
}
