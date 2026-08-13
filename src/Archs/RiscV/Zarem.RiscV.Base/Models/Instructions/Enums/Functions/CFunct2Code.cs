// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct2 field of a RISC-V instruction.
/// </summary>
public enum CFunct2Code : byte
{
#pragma warning disable CS1591

    // CA
    Subtract = 0b00,
    Xor = 0b01,
    Or = 0b10,
    And = 0b11,

    // CB
    ShiftRightLogicalImmediate = 0b00,
    ShiftRightArithmeticImmediate = 0b01,
    AndImmediate = 0b10,

#pragma warning restore CS1591
}
