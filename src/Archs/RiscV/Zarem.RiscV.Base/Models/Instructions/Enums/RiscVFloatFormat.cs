// Avishai Dernis 2024

using System.Text.Json.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for float-point formats in RISC-V floating point operations.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVFloatFormat
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("s")] Single = 0b00,
    [JsonStringEnumMemberName("d")] Double = 0b01,
    [JsonStringEnumMemberName("h")] Half = 0b10,
    [JsonStringEnumMemberName("q")] Quad = 0b11,

#pragma warning restore CS1591
}
