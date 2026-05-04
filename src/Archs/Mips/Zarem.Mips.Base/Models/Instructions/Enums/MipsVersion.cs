// Avishai Dernis 2024

using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Zarem.Mips.Models.Instructions.Enums;

/// <summary>
/// An enum for which version(s) a MIPS feature is supported.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsVersion : byte
{
#pragma warning disable CS1591

    // --- The Classic Era (MIPS I / II) ---
    [XmlEnum("mips1")] [JsonStringEnumMemberName("MipsI")] MipsI = 1,
    [XmlEnum("mips2")] [JsonStringEnumMemberName("MipsII")] MipsII = 2,

    // --- The Early 64 Bit Era ( III - V ) ---
    [XmlEnum("mips3")][JsonStringEnumMemberName("MipsIII")] MipsIII = 3,
    [XmlEnum("mips3_32bit")] MipsIII_32Bit = 4,
    
    [XmlEnum("mips4")] [JsonStringEnumMemberName("MipsIV")] MipsIV = 5,
    [XmlEnum("mips4_32bit")] MipsIV_32Bit = 6,

    [XmlEnum("mips5")] [JsonStringEnumMemberName("MipsV")] MipsV = 7,
    [XmlEnum("mips5_32bit")] MipsV_32Bit = 8,

    // --- The MIPS 32/64 Era ---
    [JsonStringEnumMemberName("Mips_R1")] Mips_R1 = 20,
    [XmlEnum("mips32r1")] Mips32R1 = 21,
    [XmlEnum("mips64r1")] Mips64R1 = 22,

    [JsonStringEnumMemberName("Mips_R2")] Mips_R2 = 23,
    [XmlEnum("mips32r2")] Mips32R2 = 24,
    [XmlEnum("mips64r2")] Mips64R2 = 25,

    [JsonStringEnumMemberName("Mips_R3")] Mips_R3 = 26,
    [XmlEnum("mips32r3")] Mips32R3 = 27,
    [XmlEnum("mips64r3")] Mips64R3 = 28,

    // There is no official R4

    [JsonStringEnumMemberName("Mips_R5")] Mips_R5 = 29,
    [XmlEnum("mips32r5")] Mips32R5 = 30,
    [XmlEnum("mips64r5")] Mips64R5 = 31,

    // --- The "Breaking" Era ---
    [JsonStringEnumMemberName("Mips_R6")] Mips_R6 = 32,
    [XmlEnum("mips32r6")] Mips32R6 = 33,
    [XmlEnum("mips64r6")] Mips64R6 = 34,

#pragma warning restore CS1591
}
