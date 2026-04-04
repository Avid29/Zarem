// Avishai Dernis 2026

namespace Zarem.Models.Instructions.Enums.Operations;

/// <summary>
/// An enum representing the operation code (opcode) of a RISC-V instruction.
/// </summary>
public enum OperationCode : byte
{
    #pragma warning disable CS1591

    Load = 0x03,
    MiscMem = 0x0f,
    AddUpperImmateToPC = 0x17,
    AluImmediate = 0x13,
    AluImmediate32 = 0x1b,
    Store = 0x23,
    Alu = 0x33,
    Alu32 = 0x3b,
    LoadUpperImmediate = 0x37,
    AluImmediate64 = 0x4b,
    Alu64 = 0x5b,
    Branch = 0x63,
    Jalr = 0x67,
    JumpAndLink = 0x6f,
    System = 0x73,

#pragma warning restore CS1591
}
