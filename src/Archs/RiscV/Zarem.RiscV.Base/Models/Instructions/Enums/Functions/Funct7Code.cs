// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct7 field of a RISC-V instruction.
/// </summary>
public enum Funct7Code : byte
{
    /// <summary>
    /// The default value for most R-Type instructions (ADD, SLL, SLT, SRL, XOR, OR, AND).
    /// </summary>
    Base = 0x00,

    /// <summary>
    /// Used to modify the operation (SUB instead of ADD, SRA instead of SRL).
    /// This effectively toggles bit 30 of the instruction.
    /// </summary>
    Modified = 0x20,

    /// <summary>
    /// Multiplier and Divider extension (MUL, MULH, DIV, REM, etc).
    /// </summary>
    /// <remarks>
    /// Used for the RV32M extension.
    /// </remarks>
    MExtension = 0x01,
}
