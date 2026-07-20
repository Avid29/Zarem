// Avishai Dernis 2026

namespace Zarem.RiscV.Emulator.Enums;

/// <summary>
/// An enum for the float classification types.
/// </summary>
public enum FloatClassification : ushort
{
#pragma warning disable CS1591

    None = 0x0,
    NegativeInfinity = 0x1,
    NegativeNormal = 0x2,
    NegativeSubnormal = 0x4,
    NegativeZero = 0x8,
    PositiveZero = 0x10,
    PositiveSubnormal = 0x20,
    PositiveNormal = 0x40,
    PositiveInfinity = 0x80,
    SignalingNaN = 0x100,
    QuietNaN = 0x200,

#pragma warning restore CS1591
}
