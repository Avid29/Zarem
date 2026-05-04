// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Operations;

/// <summary>
/// An enum representing the operation code (opcode) of a RISC-V instruction.
/// </summary>
public enum RiscVOpCode : byte
{
#pragma warning disable CS1591
    Load = 0x03,
    FloatLoad = 0x07,
    MiscMem = 0x0f,
    AddUpperImmediateToPC = 0x17,
    OpImmediate = 0x13,
    OpImmediate32 = 0x1b,
    Store = 0x23,
    FloatStore = 0x27,
    Op = 0x33,
    Op32 = 0x3b,
    LoadUpperImmediate = 0x37,
    FloatMultiplyAdd = 0x43,
    FloatMultiplySub = 0x47,
    FloatNegMultiplySub = 0x4b,
    OpImmediate64 = 0x4b,
    FloatNegMultiplyAdd = 0x4f,
    FloatCompute = 0x53,
    Op64 = 0x5b,
    Branch = 0x63,
    JumpAndLinkRegister = 0x67,
    JumpAndLink = 0x6f,
    System = 0x73,

#pragma warning restore CS1591
}
