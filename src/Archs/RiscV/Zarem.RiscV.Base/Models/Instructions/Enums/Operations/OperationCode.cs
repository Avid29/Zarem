// Avishai Dernis 2026

namespace Zarem.Models.Instructions.Enums.Operations;

/// <summary>
/// An enum representing the operation code (opcode) of a RISC-V instruction.
/// </summary>
public enum OperationCode : byte
{
    // --- I-Type (Immediate Arithmetic & Loads) ---

    /// <summary>
    /// Load instructions (LB, LH, LW, LBU, LHU).
    /// </summary>
    Load = 0x03,

    /// <summary>
    /// Arithmetic with immediates (ADDI, SLTI, XORI, ORI, ANDI, SLLI, SRLI, SRAI).
    /// </summary>
    AluImmediate = 0x13,

    /// <summary>
    /// Jump and Link Register (JALR).
    /// </summary>
    Jalr = 0x67,  // 0x67

    // --- R-Type (Register-Register Arithmetic) ---
    /// <summary>
    /// Arithmetic between registers (ADD, SUB, SLL, SLT, SLTU, XOR, SRL, SRA, OR, AND).
    /// </summary>
    Alu = 0x33,    // 0x33

    // --- S-Type (Stores) ---
    /// <summary>
    /// Store instructions (SB, SH, SW).
    /// </summary>
    Store = 0x23, // 0x23

    // --- B-Type (Branches) ---
    /// <summary>
    /// Conditional branch instructions (BEQ, BNE, BLT, BGE, BLTU, BGEU).
    /// </summary>
    Branch = 0x63, // 0x63

    // --- U-Type (Upper Immediates) ---

    /// <summary>
    /// Load Upper Immediate (LUI).
    /// </summary>
    LoadUpperImmediate = 0x37,

    /// <summary>
    /// Add Upper Immediate to PC (AUIPC).
    /// </summary>
    AddUpperImmateToPC = 0x17,

    // --- J-Type (Unconditional Jump) ---
    /// <summary>
    /// Jump and Link (JAL).
    /// </summary>
    JumpAndLink = 0x6f,

    // --- System / Miscellaneous ---
    /// <summary>
    /// System calls and CSR instructions (ECALL, EBREAK, CSRRW, etc).
    /// </summary>
    System = 0x73, // 0x73

    /// <summary>
    /// Memory ordering/fencing (FENCE).
    /// </summary>
    MiscMem = 0x0f // 0x0F
}
