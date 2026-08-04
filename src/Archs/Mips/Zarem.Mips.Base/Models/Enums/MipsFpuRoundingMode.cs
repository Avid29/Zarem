// Avishai Dernis 2026

namespace Zarem.Mips.Models.Enums;

/// <summary>
/// Specifies the MIPS FPU IEEE 754 rounding mode selection (FCSR bits 1:0).
/// </summary>
public enum MipsFpuRoundingMode : byte
{
#pragma warning disable CS1591

    RoundToNearest,
    RoundTowardZero,
    RoundUp,
    RoundDown,

#pragma warning restore CS1591
}
