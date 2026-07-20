// Avishai Dernis 2026

using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Zarem.Mips.Models.Versioning.Enums;

/// <summary>
/// An enum for the MIPS base version.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsBaseVersion : byte
{
#pragma warning disable CS1591

    [XmlEnum("mips1")][JsonStringEnumMemberName("mips1")] MipsI = 1,
    [XmlEnum("mips2")][JsonStringEnumMemberName("mips2")] MipsII = 2,
    [XmlEnum("mips3")][JsonStringEnumMemberName("mips3")] MipsIII = 3,
    [XmlEnum("mips4")][JsonStringEnumMemberName("mips4")] MipsIV = 4,
    [XmlEnum("mips5")][JsonStringEnumMemberName("mips5")] MipsV = 5,

    [XmlEnum("mips1")][JsonStringEnumMemberName("r1")] R1 = 6,
    [XmlEnum("mips1")][JsonStringEnumMemberName("r2")] R2 = 7,
    [XmlEnum("mips1")][JsonStringEnumMemberName("r3")] R3 = 8,
    [XmlEnum("mips1")][JsonStringEnumMemberName("r5")] R5 = 9,
    [XmlEnum("mips1")][JsonStringEnumMemberName("r6")] R6 = 10,

#pragma warning restore CS1591
}
