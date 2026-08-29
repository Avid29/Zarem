// Avishai Dernis 2026

namespace Zarem.AArch64.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for AArch64 register categories.
/// </summary>
public enum AArch64RegisterCategory
{
#pragma warning disable CS1591

    Special,
    Saved,
    Temporary,
    Argument,
    Platform,
    IndirectResult,

#pragma warning restore CS1591
}
