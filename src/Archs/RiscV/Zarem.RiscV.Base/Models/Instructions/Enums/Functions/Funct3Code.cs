// Avishai Dernis 2026

namespace Zarem.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct3 field of a RISC-V instruction.
/// </summary>
public enum Funct3Code : byte
{
#pragma warning disable CS1591

    // --- ALU Operations (OP and OP-IMM) ---
    Arithmetic = 0b000,
    ShiftLeft = 0b001,
    SetLessThan = 0b010,
    SetLessU = 0b011,
    Xor = 0b100,
    ShiftRight = 0b101,
    Or = 0b110,
    And = 0b111,

    // --- Branch Operations (BRANCH) ---
    BrancEqual = 0b000,
    BranchNotEqual = 0b001,
    BranchLessThan = 0b100,
    BranchGreaterThanOrEqual = 0b101,
    BranchLessThanUnsigned = 0b110,
    BranchGreaterThanOrEqualUnsigned = 0b111,

    // --- Load Operations (LOAD) ---
    LoadByte = 0b000,
    LoadHalfWord = 0b001,
    LoadWord = 0b010,
    LoadByteUnsigned = 0b100,
    LoadHalfWordUnsigned = 0b101,

    // --- Store Operations (STORE) ---
    StoreByte = 0b000,
    StoreHalfWord = 0b001,
    StoreWord = 0b010,

    // --- System / CSR Operations (SYSTEM) ---
    EcallBreak = 0b000,     // ECALL, EBREAK
    Csrrw = 0b001,
    Csrrs = 0b010,
    Csrrc = 0b011,
    Csrrwi = 0b101,
    Csrrsi = 0b110,
    Csrrci = 0b111

#pragma warning restore CS1591
}
