// Avishai Dernis 2024

using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for which version(s) a MIPS feature is supported.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsVersion : byte
{
#pragma warning disable CS1591

    // --- The Classic Era (MIPS I - V) ---
    [XmlEnum("mips1")] [JsonStringEnumMemberName("MipsI")] MipsI = 1,
    [XmlEnum("mips2")] [JsonStringEnumMemberName("MipsII")] MipsII = 2,
    [XmlEnum("mips3")] [JsonStringEnumMemberName("MipsIII")] MipsIII = 3,
    [XmlEnum("mips4")] [JsonStringEnumMemberName("MipsIV")] MipsIV = 4,
    [XmlEnum("mips5")] [JsonStringEnumMemberName("MipsV")] MipsV = 5,

    // --- The MIPS 32/64 Era ---
    [XmlEnum("mips32r1")] [JsonStringEnumMemberName("Mips32R1")] Mips32R1 = 11,
    [XmlEnum("mips32r2")] [JsonStringEnumMemberName("Mips32R2")] Mips32R2 = 12,
    [XmlEnum("mips32r3")] [JsonStringEnumMemberName("Mips32R3")] Mips32R3 = 13,
    // There is no R4
    [XmlEnum("mips32r5")] [JsonStringEnumMemberName("Mips32R5")] Mips32R5 = 15,

    // --- The "Breaking" Era ---
    [XmlEnum("mips32r6")] [JsonStringEnumMemberName("Mips32R6")] Mips32R6 = 16,

#pragma warning restore CS1591
}
