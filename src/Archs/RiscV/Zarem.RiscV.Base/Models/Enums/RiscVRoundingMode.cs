// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.RiscV.Models.Enums;

/// <summary>
/// An enum representing the rounding mode for floating-point instructions in RISC-V.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVRoundingMode : byte
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("rne")] RoundToNearestEven = 0b000,
    [JsonStringEnumMemberName("rtz")] RoundTowardsZero = 0b001,
    [JsonStringEnumMemberName("rdn")] RoundDown = 0b010,
    [JsonStringEnumMemberName("rup")] RoundUp = 0b011,
    [JsonStringEnumMemberName("rmm")] RoundToNearestMaxMagnitude = 0b100,
    [JsonStringEnumMemberName("dyn")] Dynamic = 0b111,

#pragma warning restore CS1591
}
