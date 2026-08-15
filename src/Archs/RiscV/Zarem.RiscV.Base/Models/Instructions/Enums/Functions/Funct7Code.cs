// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct7 field of a RISC-V instruction.
/// </summary>
public enum Funct7Code : byte
{
    #region Base Integer Operations (I)

    /// <summary>
    /// The default value for most R-Type instructions (ADD, SLL, SLT, SRL, XOR, OR, AND).
    /// </summary>
    Base = 0x00,

    /// <summary>
    /// Used to modify the operation (SUB instead of ADD, SRA instead of SRL).
    /// This effectively toggles bit 30 of the instruction.
    /// </summary>
    Modified = 0x20,

    #endregion

    #region Multiply & Divide Extension (M)

    /// <summary>
    /// Multiplier and Divider extension (MUL, MULH, DIV, REM, etc).
    /// </summary>
    /// <remarks>
    /// Used for the RV32M extension.
    /// </remarks>
    MExtension = 0x1,

    #endregion

    #region Bit Manipulation Extensions (B, Zba, Zbb, Zbc, Zbs)

    /// <summary>
    /// Zbb/Zbc min/max and carryless multiply operations (MIN, MAX, MINU, MAXU, CLMUL, CLMULH, CLMULR).
    /// </summary>
    MinMaxClmul = 0x5,

    /// <summary>
    /// Zba address generation shift-add operations (SH1ADD, SH2ADD, SH3ADD, SH1ADD.UW, SH2ADD.UW, SH3ADD.UW).
    /// </summary>
    ShiftAdd = 0x10,

    /// <summary>
    /// Zbb byte-reverse/or-combine (ORC.B, REV8) and Zbs single-bit set (BSET).
    /// </summary>
    BitManipulationMisc = 0x14,

    /// <summary>
    /// Zbs single-bit clear and extract operations (BCLR, BEXT).
    /// </summary>
    ZbsBClrBExt = 0x24,

    /// <summary>
    /// Zbb bit count/rotate operations (CLZ, CTZ, CPOP, ROL, ROR, SEXT.B, SEXT.H).
    /// </summary>
    BitManipCountRotate = 0x30,

    /// <summary>
    /// Zbs single-bit invert operation (BINV).
    /// </summary>
    ZbsBInv = 0x34,

    #endregion
}
