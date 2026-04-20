// Avishai Dernis 2025

using System.Text.Json.Serialization;

namespace Zarem.Models.Instructions.Enums;

/// <summary>
/// An enum for argument types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsArgument
{
#pragma warning disable CS1591

    // General Registers
    [JsonStringEnumMemberName("rs")] RS,
    [JsonStringEnumMemberName("rt")] RT,
    [JsonStringEnumMemberName("rd")] RD,

    [JsonStringEnumMemberName("sa")] ShiftAmount,

    /// <summary>
    /// The 16 bit immediate value.
    /// </summary>
    [JsonStringEnumMemberName("imm")] Immediate,

    /// <summary>
    /// A branch's offset.
    /// </summary>
    [JsonStringEnumMemberName("offset")] Offset,

    /// <summary>
    /// The 26-bit immediate value.
    /// </summary>
    [JsonStringEnumMemberName("target")] Address,

    /// <summary>
    /// An base memory address from a register, and a 16-bit offset.
    /// </summary>
    [JsonStringEnumMemberName("offset_rs")] AddressBase,

    /// <summary>
    /// A 32 bit immediate value.
    /// </summary>
    [JsonStringEnumMemberName("imm32")] FullImmediate,

    // Floating Point Registers
    [JsonStringEnumMemberName("fs")] FS,
    [JsonStringEnumMemberName("ft")] FT,
    [JsonStringEnumMemberName("fd")] FD,

    // RS/RT Register argument for coprocessors. Must use numbered register name.
    [JsonStringEnumMemberName("rs_num")] RS_Numbered,
    [JsonStringEnumMemberName("rt_num")] RT_Numbered,

    /// <summary>
    /// A 26-bit branch offset.
    /// </summary>
    [JsonStringEnumMemberName("offset26")] LargeOffset,

#pragma warning restore CS1591
}
