// Avishai Dernis 2026

namespace Zarem.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum representing the rounding mode for floating-point instructions in RISC-V.
/// </summary>
public enum RiscVRoundingMode : byte
{
#pragma warning disable CS1591

    RoundToNearest = 0b000,
    RoundTowardsZero = 0b001,
    RoundDown = 0b010,
    RoundUp = 0b011,
    RoundToNearestMaxMagnitude = 0b100,
    Dynamic = 0b111,

#pragma warning restore CS1591
}
