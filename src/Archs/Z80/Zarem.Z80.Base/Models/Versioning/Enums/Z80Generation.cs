// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.Z80.Models.Versioning.Enums;

/// <summary>
/// An enum for which Z80 architecture.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Z80Generation : byte
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("Z80")] Z80,
    [JsonStringEnumMemberName("Z180")] Z180,
    [JsonStringEnumMemberName("Z280")] Z280,
    [JsonStringEnumMemberName("Z380")] Z380,

#pragma warning restore CS1591
}
