// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Operations;

/// <summary>
/// An enum representing the compression operation code (opcode) of a RISC-V instruction.
/// </summary>
public enum RiscVCompressionCode : byte
{
#pragma warning disable CS1591

    C0 = 0b00,
    C1 = 0b01,
    C2 = 0b10,
    Uncompressed = 0b11,

#pragma warning restore CS1591
}
