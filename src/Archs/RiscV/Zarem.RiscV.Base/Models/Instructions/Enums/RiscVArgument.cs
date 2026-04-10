// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for argument types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVArgument : byte
{
#pragma warning disable CS1591

    // Integer Registers
    [JsonStringEnumMemberName("rd")] RD,
    [JsonStringEnumMemberName("rs1")] RS1,
    [JsonStringEnumMemberName("rs2")] RS2,

    // Floating-Point Registers
    [JsonStringEnumMemberName("frd")] FRD,
    [JsonStringEnumMemberName("frs1")] FRS1,
    [JsonStringEnumMemberName("frs2")] FRS2,
    [JsonStringEnumMemberName("frs3")] FRS3,

    // Immediates
    [JsonStringEnumMemberName("imm")] Immediate,
    [JsonStringEnumMemberName("store_offset")] StoreOffset,
    [JsonStringEnumMemberName("branch_offset")] BranchOffset,
    [JsonStringEnumMemberName("upper_imm")] UpperImmediate,
    [JsonStringEnumMemberName("jump_offset")] JumpOffset,

    // System
    [JsonStringEnumMemberName("csr")] Csr,      // 12-bit CSR address
    [JsonStringEnumMemberName("csri")] UImm5,    // 5-bit immediate for CSRI

    // Memory syntax (e.g., 8(sp))
    [JsonStringEnumMemberName("mem")] Memory    // This would be a combination of Imm12I + Rs1

#pragma warning restore CS1591
}
