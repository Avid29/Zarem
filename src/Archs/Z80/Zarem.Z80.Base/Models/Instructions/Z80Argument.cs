// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Attributes.Arguments;
using Zarem.Z80.Models.Enums;

namespace Zarem.Z80.Models.Instructions;

/// <summary>
/// An enum for Z80 argument types.
/// </summary>
public enum Z80Argument
{
#pragma warning disable CS1591

    // 8-Bit Source / Destination Registers (b000 - b111 encodings)
    [JsonStringEnumMemberName("r_dest")]
    [RegisterArgument<Z80RegisterSet>("r_dest", Z80RegisterSet.GeneralPurpose8)]
    RDest,

    [JsonStringEnumMemberName("r_src")]
    [RegisterArgument<Z80RegisterSet>("r_src", Z80RegisterSet.GeneralPurpose8)]
    RSrc,

    // 16-Bit Register Pairs (b00 - b11 encodings)
    [JsonStringEnumMemberName("dd")]
    [RegisterArgument<Z80RegisterSet>("dd", Z80RegisterSet.RegisterPairsStandard)]
    DD,

    [JsonStringEnumMemberName("qq")]
    [RegisterArgument<Z80RegisterSet>("qq", Z80RegisterSet.RegisterPairsStack)]
    QQ,

    // Immediates
    [JsonStringEnumMemberName("imm8")]
    [ImmediateArgument<Z80ReferenceType>("immediate", 8, false)]
    Immediate8, // Standard 8-bit literal / port (e.g., LD A, n)

    [JsonStringEnumMemberName("imm16")]
    [ImmediateArgument<Z80ReferenceType>("immediate", 16, false, DefaultRelocation = Z80ReferenceType.Absolute16)]
    Immediate16, // Standard 16-bit address or absolute pointer (e.g., LD HL, nn)

    [JsonStringEnumMemberName("rel_offset")]
    [ImmediateArgument<Z80ReferenceType>("offset", 8, true, DefaultRelocation = Z80ReferenceType.Relative8)]
    RelativeOffset, // Signed 8-bit PC displacement (e.g., JR e, DJNZ e)

    // Memory Syntax & Indirection
    [JsonStringEnumMemberName("indirect_imm16")]
    [ImmediateArgument<Z80ReferenceType>("address", 16, false)]
    IndirectImmediate16, // (nn) syntax, like LD A, (1234h)

    // Index Register Displacements (e.g., (IX+d) or (IY+d))
    [JsonStringEnumMemberName("idx_offset_ix")]
    [SplitArgument<Z80Argument>("(ix+d)", RDest, Immediate8)]
    IndexOffsetIX, // Handled when 0xDD prefix substitutes HL

    [JsonStringEnumMemberName("idx_offset_iy")]
    [SplitArgument<Z80Argument>("(iy+d)", RDest, Immediate8)]
    IndexOffsetIY  // Handled when 0xFD prefix substitutes HL

#pragma warning restore CS1591
}
