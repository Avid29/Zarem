// Avishai Dernis 2024

using System.Text.Json.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for float-point formats in floating point operations.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsFloatFormat
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("s")] Single = 0x10,
    [JsonStringEnumMemberName("d")] Double = 0x11,
    [JsonStringEnumMemberName("w")] Word = 0x14,
    [JsonStringEnumMemberName("l")] Long = 0x15,
    [JsonStringEnumMemberName("ps")] PairedSingle = 0x16,

#pragma warning restore CS1591
}
