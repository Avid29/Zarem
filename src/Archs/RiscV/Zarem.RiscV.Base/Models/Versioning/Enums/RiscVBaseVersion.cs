// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.RiscV.Models.Versioning.Enums;

/// <summary>
/// An enum for which RISC-V Base ISA or major version a feature is supported.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVBaseVersion : byte
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("RV32")] RV32 = 0,
    [JsonStringEnumMemberName("RV64")] RV64 = 1,
    [JsonStringEnumMemberName("RV128")] RV128 = 2,

#pragma warning restore CS1591
}
