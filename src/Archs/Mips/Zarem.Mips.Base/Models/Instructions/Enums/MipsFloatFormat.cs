// Avishai Dernis 2024

using System.Text.Json.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for float-point formats in MIPS floating point operations.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsFloatFormat : byte
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("S")] Single = 0x10,
    [JsonStringEnumMemberName("D")] Double = 0x11,
    [JsonStringEnumMemberName("W")] Word = 0x14,
    [JsonStringEnumMemberName("L")] Long = 0x15,
    [JsonStringEnumMemberName("PS")] PairedSingle = 0x16,

#pragma warning restore CS1591
}
