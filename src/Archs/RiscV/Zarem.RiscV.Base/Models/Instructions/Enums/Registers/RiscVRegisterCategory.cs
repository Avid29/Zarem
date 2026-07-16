// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for RISC-V register categories.
/// </summary>
public enum RiscVRegisterCategory
{
#pragma warning disable CS1591

    Special,
    Saved,
    Temporary,
    Argument,

    FloatSaved,
    FloatTemporary,
    FloatArgument,

#pragma warning restore CS1591
}
