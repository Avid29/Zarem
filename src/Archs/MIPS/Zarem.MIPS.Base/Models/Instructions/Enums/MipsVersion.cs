// Avishai Dernis 2024

using System.Xml.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for which version(s) a MIPS feature is supported.
/// </summary>
public enum MipsVersion : byte
{
#pragma warning disable CS1591

    [XmlEnum(Name = "mips1")]
    MipsI = 1,

    [XmlEnum(Name = "mips2")]
    MipsII = 2,

    [XmlEnum(Name = "mips3")]
    MipsIII = 3,

    [XmlEnum(Name = "mips4")]
    MipsIV = 4,

    [XmlEnum(Name = "mips5")]
    MipsV = 5,

    [XmlEnum(Name = "mips6")]
    MipsVI = 6, 

#pragma warning restore CS1591
}
