// Avishai Dernis 2026

namespace Zarem.RiscV.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the funct3 field of a RISC-V floating-point instruction.
/// </summary>
public enum FloatFunct3Code : byte
{
#pragma warning disable CS1591

    // --- Rounding Modes (Used by Arithmetic & Conversions) ---
    RoundToNearest = RiscVRoundingMode.RoundToNearest,
    RoundTowardsZero = RiscVRoundingMode.RoundTowardsZero,
    RoundDown = RiscVRoundingMode.RoundDown,
    RoundUp = RiscVRoundingMode.RoundUp,
    RoundToNearestMaxMagnitude = RiscVRoundingMode.RoundToNearestMaxMagnitude,
    Dynamic = RiscVRoundingMode.Dynamic,

    // --- Sign Injection (FSGNJ.fmt) ---
    FloatSignInject = 0b000,        // FSGNJ
    FloatSignInjectNegated = 0b001, // FSGNJN
    FloatSignInjectXor = 0b010,     // FSGNJX

    // --- Minimum/Maximum (FMIN.fmt / FMAX.fmt) ---
    FloatMin = 0b000,
    FloatMax = 0b001,

    // --- Comparisons (FEQ / FLT / FLE) ---
    FloatEqual = 0b010,
    FloatLessThan = 0b001,
    FloatLessOrEqual = 0b000,

    // --- Classify / Move (FCLASS / FMV) ---
    FloatClassifyOrMove = 0b000,

#pragma warning restore CS1591
}
