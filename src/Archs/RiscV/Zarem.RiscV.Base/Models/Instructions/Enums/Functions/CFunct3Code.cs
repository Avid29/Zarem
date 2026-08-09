// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct3 field of a RISC-V instruction.
/// </summary>
public enum CFunct3Code : byte
{
#pragma warning disable CS1591

    // Quadrant 0
    AddImmediate4spn = 0b000,
    LoadDouble = 0b001,
    LoadWord = 0b010,
    LoadDoubleWord = 0b011,                 // RV64/128C
    LoadSingle = 0b011,                     // RV32C
    StoreDouble = 0b101,
    StoreWord = 0b110,
    StoreDoubleWord = 0b111,                // RV64/128C
    StoreSingle = 0b111,                    // RV32C

    // Quadrant 1
    AddImmediate = 0b000,
    NoOp = 0b000,
    JumpAndLink = 0b001,                    // RV32C
    AddImmediateWide = 0b001,               // RV64/128C
    LoadImmediate = 0b010,
    AddImmediate16SP = 0b011,
    LoadUpperImmediate = 0b011,
    MiscAlu = 0b100,
    Jump = 0b101,
    BranchOnEqualToZero = 0b110,
    BranchOnNotEqualToZero = 0b111,

    // Quadrant 2
    ShiftLeftLogicalImmediate = 0b000,
    LoadDoubleStackPointer = 0b001,
    LoadWordStackPointer = 0b010,
    LoadDoubleWordStackPointer = 0b011,     // RV64/128C
    LoadSingleStackPointer = 0b011,         // RV32C
    RegisterOp = 0b100,
    StoreDoubleStackPointer = 0b101,
    StoreWordStackPointer = 0b110,
    StoreDoubleWordStackPointer = 0b111,    // RV64/128C
    StoreSingleStackPointer = 0b111,        // RV32C

#pragma warning restore CS1591
}
