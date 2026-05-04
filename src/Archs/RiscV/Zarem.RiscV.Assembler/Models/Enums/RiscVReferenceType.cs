// Avishai Dernis 2026

namespace Zarem.RiscV.Assembler.Models.Enums;

/// <summary>
/// An enum for RISC-V relocation and reference types.
/// </summary>
public enum RiscVReferenceType : uint
{
    /// <summary>
    /// No relocation.
    /// </summary>
    None = 0,

    /// <summary>
    /// 32-bit absolute address (e.g., for data constants).
    /// </summary>
    Absolute32 = 1,

    /// <summary>
    /// 64-bit absolute address (RV64).
    /// </summary>
    Absolute64 = 2,

    /// <summary>
    /// PC-relative 20-bit branch offset (B-type instructions).
    /// Bits are scrambled in the instruction but represent a 13-bit signed immediate.
    /// </summary>
    Branch20 = 3,

    /// <summary>
    /// PC-relative 20-bit jump offset (J-type instructions / JAL).
    /// Represents a 21-bit signed immediate.
    /// </summary>
    Jump20 = 4,

    /// <summary>
    /// High 20-bits of a 32-bit absolute address (%hi).
    /// Usually used with LUI.
    /// </summary>
    High20 = 5,

    /// <summary>
    /// Low 12-bits of a 32-bit absolute address (%lo).
    /// Used with I-type (addi, lw) or S-type (sw) instructions.
    /// </summary>
    Low12 = 6,

    /// <summary>
    /// High 20-bits of a PC-relative displacement (%pcrel_hi).
    /// Used with AUIPC.
    /// </summary>
    PCRelativeHigh20 = 7,

    /// <summary>
    /// Low 12-bits of a PC-relative displacement (%pcrel_lo).
    /// Points to the corresponding AUIPC instruction rather than the symbol.
    /// </summary>
    PCRelativeLow12 = 8,

    /// <summary>
    /// High 20-bits of a PC-relative offset to a GOT entry (%got_pcrel_hi).
    /// </summary>
    GlobalOffsetTableHigh20 = 9,

    /// <summary>
    /// PC-relative call (PLT). Usually expands to an AUIPC/JALR pair.
    /// </summary>
    Call = 10,

    /// <summary>
    /// Thread-local storage high 20-bits (%tprel_hi).
    /// </summary>
    TPRelativeHigh20 = 11,

    /// <summary>
    /// Thread-local storage low 12-bits (%tprel_lo).
    /// </summary>
    TPRelativeLow12 = 12,

    /// <summary>
    /// Local label reference (used for linker relaxation alignment).
    /// </summary>
    Relax = 13
}
