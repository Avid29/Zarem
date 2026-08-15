// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct4 field of a RISC-V instruction.
/// </summary>
public enum CFunct4Code : byte
{
#pragma warning disable CS1591

    JumpRegister = 0b1000,          // rs2 == 0
    Move = 0b1000,                  // rs2 != 0

    JumpAndLinkRegister = 0b1001,   // rs1 != 0 && rs2 == 0
    Add = 0b1001,                   // rs1 != 0 && rs2 != 0
    EnvironmentBreak = 0b1001,      // rs1 == 0 && rs2 == 0

#pragma warning restore CS1591
}
