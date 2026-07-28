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

    // --- Base Integer Sets (The Foundation) ---
    // RV32I is the most common target for hobbyist emulators.
    [JsonStringEnumMemberName("RV32")] RV32 = 1,

    // RV32E is the "Embedded" variant with only 16 registers (x0-x15).
    //[JsonStringEnumMemberName("RV32E")] RV32E = 2,

    // 64-bit and 128-bit base sets.
    [JsonStringEnumMemberName("RV64")] RV64 = 10,
    [JsonStringEnumMemberName("RV128")] RV128 = 20,

    //// --- Historical / Profile Versions ---
    //// RISC-V now uses "Profiles" (e.g., RVA20, RVA22) for platform compatibility.
    //[JsonStringEnumMemberName("RVA20")] RVA20 = 40,
    //[JsonStringEnumMemberName("RVA22")] RVA22 = 41,
    //[JsonStringEnumMemberName("RVM23")] RVM23 = 42,

#pragma warning restore CS1591
}
