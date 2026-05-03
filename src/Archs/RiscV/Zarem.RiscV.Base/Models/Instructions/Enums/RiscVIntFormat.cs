// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for integer formats in RISC-V floating-point operations.
/// </summary>
public enum RiscVIntFormat : byte
{
#pragma warning disable CS1591
    [JsonStringEnumMemberName("w")] Word = 0b00,
    [JsonStringEnumMemberName("wu")] WordUnsigned = 0b01,
    [JsonStringEnumMemberName("l")] Long = 0b10,
    [JsonStringEnumMemberName("lu")] LongUnsigned = 0b11

#pragma warning restore CS1591
}
