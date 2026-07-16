// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Attributes.Arguments;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Models.Instructions.Enums;

/// <summary>
/// An enum for argument types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVArgument : byte
{
#pragma warning disable CS1591

    // Integer Registers
    [JsonStringEnumMemberName("rd")]
    [RegisterArgument<RiscVRegisterSet>(RiscVRegisterSet.GeneralPurpose)]
    RD,
    
    [JsonStringEnumMemberName("rs1")]
    [RegisterArgument<RiscVRegisterSet>(RiscVRegisterSet.GeneralPurpose)]
    RS1,
    
    [JsonStringEnumMemberName("rs2")]
    [RegisterArgument<RiscVRegisterSet>(RiscVRegisterSet.GeneralPurpose)]
    RS2,

    // Floating-Point Registers
    [JsonStringEnumMemberName("frd")]
    [RegisterArgument<RiscVRegisterSet>(RiscVRegisterSet.FloatingPoints)]
    FRD,

    [JsonStringEnumMemberName("frs1")]
    [RegisterArgument<RiscVRegisterSet>(RiscVRegisterSet.FloatingPoints)]
    FRS1,

    [JsonStringEnumMemberName("frs2")]
    [RegisterArgument<RiscVRegisterSet>(RiscVRegisterSet.FloatingPoints)]
    FRS2,

    [JsonStringEnumMemberName("frs3")]
    [RegisterArgument<RiscVRegisterSet>(RiscVRegisterSet.FloatingPoints)]
    FRS3,

    // Immediates
    [JsonStringEnumMemberName("imm")]
    [ImmediateArgument(12, true)]
    Immediate,
    
    [JsonStringEnumMemberName("store_offset")]
    [ImmediateArgument(12, true)]
    StoreOffset,
    
    [JsonStringEnumMemberName("branch_offset")]
    [ImmediateArgument(12, true, 1)]
    BranchOffset,
    
    [JsonStringEnumMemberName("upper_imm")]
    [ImmediateArgument(20, false)]
    UpperImmediate,
    
    [JsonStringEnumMemberName("jump_offset")]
    [ImmediateArgument(20, true, 1)]
    JumpOffset,
    
    [JsonStringEnumMemberName("imm32")]
    [ImmediateArgument(32, true)]
    FullImmediate,

    // System
    [JsonStringEnumMemberName("csr")]
    [ImmediateArgument(12, false)]
    Csr,      // 12-bit CSR address
    
    [JsonStringEnumMemberName("csri")]
    [ImmediateArgument(5, false, 1)]
    UImm5,    // 5-bit immediate for CSRI

    // Memory syntax (e.g., 8(sp))
    [JsonStringEnumMemberName("mem_load")]
    [SplitArgument<RiscVArgument>(RS1, Immediate)]
    MemoryLoad,    // This would be a combination of Immediate + RS1
    
    [JsonStringEnumMemberName("mem_store")]
    [SplitArgument<RiscVArgument>(RS1, StoreOffset)]
    MemoryStore    // This would be a combination of StoreOffset + RS1

#pragma warning restore CS1591
}
