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
    /// Zba word unsigned add and shift operations (ADD.UW, SLLI.UW).
    /// </summary>
    ZbaAddUw = 0x4,

    /// <summary>
    /// Zbb/Zbc min/max and carryless multiply operations (MIN, MAX, MINU, MAXU, CLMUL, CLMULH, CLMULR).
    /// </summary>
    MinMaxClmul = 0x5,

    /// <summary>
    /// Zba address generation shift-add operations (SH1ADD, SH2ADD, SH3ADD, SH1ADD.UW, SH2ADD.UW, SH3ADD.UW).
    /// </summary>
    ShiftAdd = 0x10,

    /// <summary>
    /// Zbb OR-combine operation (ORC.B).
    /// </summary>
    ZbbOrcB = 0x14,

    /// <summary>
    /// Zbb inverted bitwise logic operations (ANDN, ORN, XNOR).
    /// </summary>
    BitwiseLogicInverted = 0x20,

    /// <summary>
    /// Zbs single-bit clear operation (BCLR, BCLRI).
    /// </summary>
    ZbsBClr = 0x24,

    /// <summary>
    /// Zbs single-bit set operation (BSET, BSETI).
    /// </summary>
    ZbsBSet = 0x28,

    /// <summary>
    /// Zbb bit count/rotate and sign-extension operations (CLZ, CTZ, CPOP, ROL, ROR, RORI, SEXT.B, SEXT.H).
    /// </summary>
    BitManipulationCountRotate = 0x30,

    /// <summary>
    /// Zbb byte-reverse operation (REV8).
    /// </summary>
    ZbbRev8 = 0x34,

    /// <summary>
    /// Zbs single-bit extract operation (BEXT, BEXTI).
    /// </summary>
    ZbsBExt = 0x48,

    /// <summary>
    /// Zbs single-bit invert operation (BINV, BINVI).
    /// </summary>
    ZbsBInv = 0x68,

    #endregion
}
