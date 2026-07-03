// Avishai Dernis 2026

namespace Zarem.Mips.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for MIPS register categories.
/// </summary>
public enum MipsRegisterCategory
{
#pragma warning disable CS1591

    Special,
    Saved,
    Temporary,
    Argument,
    ReturnValue,
    Kernel,
    HighLow,

#pragma warning restore CS1591
}
