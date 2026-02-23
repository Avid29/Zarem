// Avishai Dernis 2024

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for float-point formats in floating point operations
/// </summary>
public enum FloatFormat
{
#pragma warning disable CS1591

    Single = 0x10,
    Double = 0x11,
    Word = 0x14,
    Long = 0x15,
    PairedSingle = 0x16,

#pragma warning restore CS1591
}
