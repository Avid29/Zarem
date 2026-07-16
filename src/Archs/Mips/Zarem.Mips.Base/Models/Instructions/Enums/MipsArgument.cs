// Avishai Dernis 2025

using System.Text.Json.Serialization;
using Zarem.Attributes;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Models.Instructions.Enums;

/// <summary>
/// An enum for argument types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MipsArgument
{
#pragma warning disable CS1591

    // General Registers
    [JsonStringEnumMemberName("rs")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.GeneralPurpose)]
    RS,

    [JsonStringEnumMemberName("rt")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.GeneralPurpose)]
    RT,
    
    [JsonStringEnumMemberName("rd")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.GeneralPurpose)]
    RD,

    // Floating Point Registers
    [JsonStringEnumMemberName("fs")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.FloatingPoints)]
    FS,

    [JsonStringEnumMemberName("ft")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.FloatingPoints)]
    FT,

    [JsonStringEnumMemberName("fd")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.FloatingPoints)]
    FD,

    // Immediates
    [JsonStringEnumMemberName("sa")]
    [ImmediateArgument(5, false)]
    ShiftAmount,

    [JsonStringEnumMemberName("imm")]
    [ImmediateArgument(16, true)]
    Immediate,

    [JsonStringEnumMemberName("offset")]
    [ImmediateArgument(16, true, 2)]
    Offset,

    [JsonStringEnumMemberName("target")]
    [ImmediateArgument(26, false)]
    Address,

    [JsonStringEnumMemberName("offset26")]
    [ImmediateArgument(26, false, 2)]
    LargeOffset,

    [JsonStringEnumMemberName("imm32")]
    [ImmediateArgument(32, false)]
    FullImmediate,

    // Memory syntax
    [JsonStringEnumMemberName("offset_rs")]
    [SplitArgument<MipsArgument>(RS, Offset)]
    AddressBase,

    // RS/RT Register argument for coprocessors. Must use numbered register name.
    [JsonStringEnumMemberName("rs_num")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.FloatingPoints)]
    RS_Numbered,
    
    [JsonStringEnumMemberName("rt_num")]
    [RegisterArgument<MipsRegisterSet>(MipsRegisterSet.FloatingPoints)] 
    RT_Numbered,

#pragma warning restore CS1591
}
