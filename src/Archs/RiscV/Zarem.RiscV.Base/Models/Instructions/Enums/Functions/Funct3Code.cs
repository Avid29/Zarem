// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

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
    SetLessThanUnsigned = 0b011,
    Xor = 0b100,
    ShiftRight = 0b101,
    Or = 0b110,
    And = 0b111,

    // --- Branch Operations (BRANCH) ---
    BranchEqual = 0b000,
    BranchNotEqual = 0b001,
    BranchLessThan = 0b100,
    BranchGreaterThanOrEqual = 0b101,
    BranchLessThanUnsigned = 0b110,
    BranchGreaterThanOrEqualUnsigned = 0b111,

    // --- Load Operations (LOAD) ---
    LoadByte = 0b000,
    LoadHalfWord = 0b001,
    LoadWord = 0b010,
    LoadDoubleWord = 0b011,
    LoadByteUnsigned = 0b100,
    LoadHalfWordUnsigned = 0b101,
    LoadWordUnsigned = 0b110,

    // --- Store Operations (STORE) ---
    StoreByte = 0b000,
    StoreHalfWord = 0b001,
    StoreWord = 0b010,
    StoreDoubleWord = 0b11,

    // --- System / CSR Operations (SYSTEM) ---
    EcallBreak = 0b000,     // ECALL, EBREAK
    Csrrw = 0b001,
    Csrrs = 0b010,
    Csrrc = 0b011,
    Csrrwi = 0b101,
    Csrrsi = 0b110,
    Csrrci = 0b111,

    // --- Multiplication ---
    Multiply = 0b000,
    MultiplyHigh = 0b001,
    MultiplyHighSignedUnsigned = 0b010,
    MultiplyHighUnsigned = 0b011,
    Divide = 0b100,
    DivideUnsigned = 0b101,
    Remainder = 0b110,
    RemainderUnsigned = 0b111,

    // --- Shift Add ---
    Shift1Add = 0b010,
    Shift2Add = 0b100,
    Shift3Add = 0b110,

    // --- Bit Manipulation ---
    BitCountSignExtendRotateLeft = 0b001,
    RotateRight = 0b101,
    Min = 0b100,
    MinUnsigned = 0b101,
    Max = 0b110,
    MaxUnsigned = 0b111,

    // --- Other ---
    JumpAndLinkRegister = 0b000

#pragma warning restore CS1591
}
