// Avishai Dernis 2026

using Zarem.Attributes;

namespace Zarem.RiscV.Models.Enums;

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
    [ReferenceType(32)]
    Absolute32 = 1,

    /// <summary>
    /// 64-bit absolute address (RV64).
    /// </summary>
    [ReferenceType(64)]
    Absolute64 = 2,

    /// <summary>
    /// PC-relative 12-bit branch offset (B-type instructions).
    /// Bits are scrambled in the instruction but represent a 13-bit signed immediate.
    /// </summary>
    [ReferenceType(12)]
    Branch12 = 3,

    /// <summary>
    /// PC-relative 20-bit jump offset (J-type instructions / JAL).
    /// Represents a 21-bit signed immediate.
    /// </summary>
    [ReferenceType(20)]
    Jump20 = 4,

    /// <summary>
    /// High 20-bits of a 32-bit absolute address (%hi).
    /// Usually used with LUI.
    /// </summary>
    [ReferenceType("hi", 20, ShiftAmount = 12)]
    High20 = 5,

    /// <summary>
    /// Low 12-bits of a 32-bit absolute address (%lo).
    /// Used with I-type (addi, lw) or S-type (sw) instructions.
    /// </summary>
    [ReferenceType("lo", 12)]
    Low12 = 6,

    /// <summary>
    /// High 20-bits of a PC-relative displacement (%pcrel_hi).
    /// Used with AUIPC.
    /// </summary>
    [ReferenceType("pcrel_hi", 20, ShiftAmount = 12)]
    PCRelativeHigh20 = 7,

    /// <summary>
    /// Low 12-bits of a PC-relative displacement (%pcrel_lo).
    /// Points to the corresponding AUIPC instruction rather than the symbol.
    /// </summary>
    [ReferenceType("pcrel_lo", 12)]
    PCRelativeLow12 = 8,

    /// <summary>
    /// High 20-bits of a PC-relative offset to a GOT entry (%got_pcrel_hi).
    /// </summary>
    [ReferenceType("got_pcrel_hi", 20, ShiftAmount = 12)]
    GlobalOffsetTableHigh20 = 9,

    /// <summary>
    /// PC-relative call (PLT). Usually expands to an AUIPC/JALR pair.
    /// </summary>
    Call = 10,

    /// <summary>
    /// Thread-local storage high 20-bits (%tprel_hi).
    /// </summary>
    [ReferenceType("tprel_hi", 20, ShiftAmount = 12)]
    TPRelativeHigh20 = 11,

    /// <summary>
    /// Thread-local storage low 12-bits (%tprel_lo).
    /// </summary>
    [ReferenceType("tprel_lo", 12)]
    TPRelativeLow12 = 12,

    /// <summary>
    /// Local label reference (used for linker relaxation alignment).
    /// </summary>
    Relax = 13
}
