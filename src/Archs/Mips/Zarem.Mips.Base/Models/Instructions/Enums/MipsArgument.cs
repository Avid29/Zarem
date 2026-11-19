// Avishai Dernis 2025

using System.Text.Json.Serialization;
using Zarem.Attributes.Arguments;
using Zarem.Mips.Models.Enums;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Models.Instructions.Enums;

/// <summary>
/// An enum for MIPS argument types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsArgument
{
#pragma warning disable CS1591

    // General Registers
    [JsonStringEnumMemberName("rs")]
    [RegisterArgument<MipsRegisterSet>("rs", MipsRegisterSet.GeneralPurpose)]
    RS,

    [JsonStringEnumMemberName("rt")]
    [RegisterArgument<MipsRegisterSet>("rt", MipsRegisterSet.GeneralPurpose)]
    RT,
    
    [JsonStringEnumMemberName("rd")]
    [RegisterArgument<MipsRegisterSet>("rd", MipsRegisterSet.GeneralPurpose)]
    RD,

    // Floating Point Registers
    [JsonStringEnumMemberName("fs")]
    [RegisterArgument<MipsRegisterSet>("fs", MipsRegisterSet.FloatingPoints)]
    FS,

    [JsonStringEnumMemberName("ft")]
    [RegisterArgument<MipsRegisterSet>("ft", MipsRegisterSet.FloatingPoints)]
    FT,

    [JsonStringEnumMemberName("fd")]
    [RegisterArgument<MipsRegisterSet>("fd", MipsRegisterSet.FloatingPoints)]
    FD,

    // Immediates
    [JsonStringEnumMemberName("sa")]
    [ImmediateArgument<MipsReferenceType>("shift", 5, false)]
    ShiftAmount,

    [JsonStringEnumMemberName("imm")]
    [ImmediateArgument<MipsReferenceType>("immediate", 16, true, DefaultRelocation = MipsReferenceType.Low16)]
    Immediate,

    [JsonStringEnumMemberName("offset")]
    [ImmediateArgument<MipsReferenceType>("offset", 16, true, ShiftAmount = 2, DefaultRelocation = MipsReferenceType.PCRelative16)]
    Offset,

    [JsonStringEnumMemberName("target")]
    [ImmediateArgument<MipsReferenceType>("address", 26, false, ShiftAmount = 2, DefaultRelocation = MipsReferenceType.JumpTarget26)]
    Address,

    [JsonStringEnumMemberName("offset26")]
    [ImmediateArgument<MipsReferenceType>("offset", 26, false, ShiftAmount = 2)]
    LargeOffset,

    [JsonStringEnumMemberName("imm32")]
    [ImmediateArgument<MipsReferenceType>("immediate", 32, false)]
    FullImmediate,

    // Memory syntax
    [JsonStringEnumMemberName("offset_rs")]
    [SplitArgument<MipsArgument>("offset(rs)", RS, Immediate)]
    AddressBase,

    // RS/RT Register argument for coprocessors. Must use numbered register name.
    [JsonStringEnumMemberName("rs_num")]
    [RegisterArgument<MipsRegisterSet>("rs", MipsRegisterSet.FloatingPoints)]
    RS_Numbered,
    
    [JsonStringEnumMemberName("rt_num")]
    [RegisterArgument<MipsRegisterSet>("rt", MipsRegisterSet.FloatingPoints)] 
    RT_Numbered,

#pragma warning restore CS1591
}
