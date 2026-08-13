// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct6 field of a RISC-V instruction.
/// </summary>
public enum CFunct6Code : byte
{
#pragma warning disable CS1591

    ArithmeticLogic = 0b100011,
    ArithmeticLogicW = 0b100111,

#pragma warning restore CS1591
}
