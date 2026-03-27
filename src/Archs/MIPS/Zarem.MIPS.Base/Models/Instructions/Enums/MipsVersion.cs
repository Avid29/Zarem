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

    // --- The Classic Era (MIPS I / II) ---
    [XmlEnum("mips1")] [JsonStringEnumMemberName("MipsI")] MipsI = 1,
    [XmlEnum("mips2")] [JsonStringEnumMemberName("MipsII")] MipsII = 2,

    // --- The Early 64 Bit Era ( III - V ) ---
    [XmlEnum("mips3")][JsonStringEnumMemberName("MipsIII")] MipsIII = 3,
    [XmlEnum("mips3_32bit")] [JsonStringEnumMemberName("MipsIII_32Bit")] MipsIII_32Bit = 4,
    
    [XmlEnum("mips4")] [JsonStringEnumMemberName("MipsIV")] MipsIV = 5,
    [XmlEnum("mips4_32bit")] [JsonStringEnumMemberName("MipsIV_32Bit")] MipsIV_32Bit = 6,
    [XmlEnum("mips5")] [JsonStringEnumMemberName("MipsV")] MipsV = 7,
    [XmlEnum("mips5_32bit")] [JsonStringEnumMemberName("MipsV_32Bit")] MipsV_32Bit = 8,

    // --- The MIPS 32/64 Era ---
    [XmlEnum("mips32r1")] [JsonStringEnumMemberName("Mips32R1")] Mips32R1 = 20,
    [XmlEnum("mips64r1")] [JsonStringEnumMemberName("Mips64R1")] Mips64R1 = 21,
    [XmlEnum("mips32r2")] [JsonStringEnumMemberName("Mips32R2")] Mips32R2 = 22,
    [XmlEnum("mips64r2")] [JsonStringEnumMemberName("Mips64R2")] Mips64R2 = 23,
    [XmlEnum("mips32r3")] [JsonStringEnumMemberName("Mips32R3")] Mips32R3 = 24,
    [XmlEnum("mips64r3")] [JsonStringEnumMemberName("Mips64R3")] Mips64R3 = 25,
    // There is no official R4
    [XmlEnum("mips32r5")] [JsonStringEnumMemberName("Mips32R5")] Mips32R5 = 28,
    [XmlEnum("mips64r5")] [JsonStringEnumMemberName("Mips64R5")] Mips64R5 = 29,

    // --- The "Breaking" Era ---
    [XmlEnum("mips32r6")] [JsonStringEnumMemberName("Mips32R6")] Mips32R6 = 32,
    [XmlEnum("mips64r6")] [JsonStringEnumMemberName("Mips64R6")] Mips64R6 = 33,

#pragma warning restore CS1591
}
