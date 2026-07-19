// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Attributes.Arguments;
using Zarem.RiscV.Models.Enums;
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
    [ImmediateArgument<RiscVReferenceType>(12, true, DefaultRelocation = RiscVReferenceType.Low12)]
    Immediate,
    
    [JsonStringEnumMemberName("store_offset")]
    [ImmediateArgument<RiscVReferenceType>(12, true, DefaultRelocation = RiscVReferenceType.Low12)]
    StoreOffset,
    
    [JsonStringEnumMemberName("branch_offset")]
    [ImmediateArgument<RiscVReferenceType>(12, true, ShiftAmount = 1, DefaultRelocation = RiscVReferenceType.Branch20)]
    BranchOffset,
    
    [JsonStringEnumMemberName("upper_imm")]
    [ImmediateArgument<RiscVReferenceType>(20, false, DefaultRelocation = RiscVReferenceType.High20)]
    UpperImmediate,
    
    [JsonStringEnumMemberName("jump_offset")]
    [ImmediateArgument<RiscVReferenceType>(20, true, ShiftAmount = 1, DefaultRelocation = RiscVReferenceType.Jump20)]
    JumpOffset,
    
    [JsonStringEnumMemberName("imm32")]
    [ImmediateArgument<RiscVReferenceType>(32, true)]
    FullImmediate,

    // System
    [JsonStringEnumMemberName("csr")]
    [ImmediateArgument<RiscVReferenceType>(12, false)]
    Csr,      // 12-bit CSR address
    
    [JsonStringEnumMemberName("csri")]
    [ImmediateArgument<RiscVReferenceType>(5, false, ShiftAmount = 1)]
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
