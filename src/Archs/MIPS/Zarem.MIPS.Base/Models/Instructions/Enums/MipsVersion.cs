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
    [XmlEnum("mips1")] [JsonStringEnumMemberName("MipsI")] MipsI = 1,   // Baseline (R2000/R3000)
    [XmlEnum("mips2")] [JsonStringEnumMemberName("MipsII")] MipsII = 2,  // Added Traps, LL/SC, Likely Branches
    [XmlEnum("mips3")] [JsonStringEnumMemberName("MipsIII")] MipsIII = 3, // 64-bit support (R4000)
    [XmlEnum("mips4")] [JsonStringEnumMemberName("MipsIV")] MipsIV = 4,  // Added MOVZ/MOVN, Floating Point MADD
    [XmlEnum("mips5")] [JsonStringEnumMemberName("MipsV")] MipsV = 5,   // Paired-Single Floats (Rarely used)

    // --- The Standardized Era ---
    [XmlEnum("mips32r1")] [JsonStringEnumMemberName("Mips32R1")] Mips32R1 = 10, // Baseline for modern 32-bit (includes MUL, CLZ)
    [XmlEnum("mips32r2")] [JsonStringEnumMemberName("Mips32R2")] Mips32R2 = 11, // Added EXT/INS (Bitfield ops)

    // --- The "Breaking" Era ---
    [XmlEnum("mips32r6")] [JsonStringEnumMemberName("Mips32R6")] Mips32R6 = 16  // REMOVED Delay Slots, reorganized opcodes

#pragma warning restore CS1591
}
