// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct3 field of a RISC-V instruction.
/// </summary>
public enum CFunct3Code : byte
{
#pragma warning disable CS1591

    // --- Load Operations ---
    LoadWord = 0b010,
    LoadDoubleWord = 0b011,             // RV64/128C
    LoadSingle = 0b011,                 // RV32C
    LoadDouble = 0b001,

    // --- Store Operations ---
    StoreWord = 0b110,
    StoreDoubleWord = 0b111,             // RV64/128
    StoreSingle = 0b111,                // RV32C
    StoreDouble = 0b101,

#pragma warning restore CS1591
}
