// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.Mips.Models.Versioning.Enums;

/// <summary>
/// An enum for the MIPS base version.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsGeneration : byte
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("mips1")] MipsI = 1,
    [JsonStringEnumMemberName("mips2")] MipsII = 2,
    [JsonStringEnumMemberName("mips3")] MipsIII = 3,
    [JsonStringEnumMemberName("mips4")] MipsIV = 4,
    [JsonStringEnumMemberName("mips5")] MipsV = 5,

    [JsonStringEnumMemberName("r1")] R1 = 6,
    [JsonStringEnumMemberName("r2")] R2 = 7,
    [JsonStringEnumMemberName("r3")] R3 = 8,
    [JsonStringEnumMemberName("r5")] R5 = 9,
    [JsonStringEnumMemberName("r6")] R6 = 10,

#pragma warning restore CS1591
}
