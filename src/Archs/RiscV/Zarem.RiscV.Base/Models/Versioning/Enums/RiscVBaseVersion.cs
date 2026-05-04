// Avishai Dernis 2026

using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Zarem.RiscV.Models.Versioning.Enums;

/// <summary>
/// An enum for which RISC-V Base ISA or major version a feature is supported.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVBaseVersion : byte
{
#pragma warning disable CS1591

    // --- Base Integer Sets (The Foundation) ---
    // RV32I is the most common target for hobbyist emulators.
    [XmlEnum("rv32")][JsonStringEnumMemberName("RV32")] RV32 = 1,

    // RV32E is the "Embedded" variant with only 16 registers (x0-x15).
    //[XmlEnum("rv32e")][JsonStringEnumMemberName("RV32E")] RV32E = 2,

    // 64-bit and 128-bit base sets.
    [XmlEnum("rv64")][JsonStringEnumMemberName("RV64")] RV64 = 10,
    [XmlEnum("rv128")][JsonStringEnumMemberName("RV128")] RV128 = 20,

    //// --- Historical / Profile Versions ---
    //// RISC-V now uses "Profiles" (e.g., RVA20, RVA22) for platform compatibility.
    //[XmlEnum("rva20")][JsonStringEnumMemberName("RVA20")] RVA20 = 40,
    //[XmlEnum("rva22")][JsonStringEnumMemberName("RVA22")] RVA22 = 41,
    //[XmlEnum("rvm23")][JsonStringEnumMemberName("RVM23")] RVM23 = 42,

#pragma warning restore CS1591
}
