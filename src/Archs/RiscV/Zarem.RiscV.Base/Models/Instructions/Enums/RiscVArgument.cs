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
    [RegisterArgument<RiscVRegisterSet>("rd", RiscVRegisterSet.GeneralPurpose)]
    RD,
    
    [JsonStringEnumMemberName("rs1")]
    [RegisterArgument<RiscVRegisterSet>("rs1", RiscVRegisterSet.GeneralPurpose)]
    RS1,
    
    [JsonStringEnumMemberName("rs2")]
    [RegisterArgument<RiscVRegisterSet>("rs2", RiscVRegisterSet.GeneralPurpose)]
    RS2,

    // Floating-Point Registers
    [JsonStringEnumMemberName("frd")]
    [RegisterArgument<RiscVRegisterSet>("frd", RiscVRegisterSet.FloatingPoints)]
    FRD,

    [JsonStringEnumMemberName("frs1")]
    [RegisterArgument<RiscVRegisterSet>("frs1", RiscVRegisterSet.FloatingPoints)]
    FRS1,

    [JsonStringEnumMemberName("frs2")]
    [RegisterArgument<RiscVRegisterSet>("frs2", RiscVRegisterSet.FloatingPoints)]
    FRS2,

    [JsonStringEnumMemberName("frs3")]
    [RegisterArgument<RiscVRegisterSet>("frs3", RiscVRegisterSet.FloatingPoints)]
    FRS3,

    // Compressed/Combined  Registers
    [JsonStringEnumMemberName("c_rd")]
    [RegisterArgument<RiscVRegisterSet>("rd`", RiscVRegisterSet.CompressedGeneralPurpose)]
    CompressedRD,

    [JsonStringEnumMemberName("c_rs1")]
    [RegisterArgument<RiscVRegisterSet>("rs1`", RiscVRegisterSet.CompressedGeneralPurpose)]
    CompressedRS1,

    [JsonStringEnumMemberName("c_rs2")]
    [RegisterArgument<RiscVRegisterSet>("rs2`", RiscVRegisterSet.CompressedGeneralPurpose)]
    CompressedRS2,

    [JsonStringEnumMemberName("rdrs1")]
    [RegisterArgument<RiscVRegisterSet>("rd/rs1", RiscVRegisterSet.GeneralPurpose)]
    RDRS1,

    [JsonStringEnumMemberName("c_rdrs1")]
    [RegisterArgument<RiscVRegisterSet>("rd`/rs1`", RiscVRegisterSet.CompressedGeneralPurpose)]
    CompressedRDRS1,

    // Immediates
    [JsonStringEnumMemberName("imm")]
    [ImmediateArgument<RiscVReferenceType>("immediate", 12, true, DefaultRelocation = RiscVReferenceType.Low12)]
    Immediate,
    
    [JsonStringEnumMemberName("store_offset")]
    [ImmediateArgument<RiscVReferenceType>("offset", 12, true, DefaultRelocation = RiscVReferenceType.Low12)]
    StoreOffset,
    
    [JsonStringEnumMemberName("branch_offset")]
    [ImmediateArgument<RiscVReferenceType>("offset", 12, true, ShiftAmount = 1, DefaultRelocation = RiscVReferenceType.Branch12)]
    BranchOffset,
    
    [JsonStringEnumMemberName("upper_imm")]
    [ImmediateArgument<RiscVReferenceType>("immediate", 20, false, DefaultRelocation = RiscVReferenceType.High20)]
    UpperImmediate,
    
    [JsonStringEnumMemberName("jump_offset")]
    [ImmediateArgument<RiscVReferenceType>("offset", 20, true, ShiftAmount = 1, DefaultRelocation = RiscVReferenceType.Jump20)]
    JumpOffset,
    
    [JsonStringEnumMemberName("imm32")]
    [ImmediateArgument<RiscVReferenceType>("immediate", 32, true)]
    FullImmediate,

    // Compressed Immediates
    [JsonStringEnumMemberName("comp_imm")]
    [ImmediateArgument<RiscVReferenceType>("immediate", 6, true)]
    CompressedImmediate,

    [JsonStringEnumMemberName("comp_branch_offset")]
    [ImmediateArgument<RiscVReferenceType>("offset", 8, true, ShiftAmount = 1)]
    CompressedBranchOffset,

    [JsonStringEnumMemberName("comp_jump_offset")]
    [ImmediateArgument<RiscVReferenceType>("offset", 11, true, ShiftAmount = 1)]
    CompressedJumpOffset,

    // System
    [JsonStringEnumMemberName("csr")]
    [ImmediateArgument<RiscVReferenceType>("immediate", 12, false)]
    Csr,      // 12-bit CSR address
    
    [JsonStringEnumMemberName("csri")]
    [ImmediateArgument<RiscVReferenceType>("immediate", 5, false)]
    UImm5,    // 5-bit immediate for CSRI

    // Memory syntax (e.g., 8(sp))
    [JsonStringEnumMemberName("mem_load")]
    [SplitArgument<RiscVArgument>("offset(rs1)", RS1, Immediate)]
    MemoryLoad,    // This would be a combination of Immediate + RS1
    
    [JsonStringEnumMemberName("mem_store")]
    [SplitArgument<RiscVArgument>("offset(rs1)", RS1, StoreOffset)]
    MemoryStore    // This would be a combination of StoreOffset + RS1

#pragma warning restore CS1591
}
