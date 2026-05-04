// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions;

namespace Zarem.Mips.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing Register-Immeditate instructions.
/// </summary>
public record RegImmInstructionMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction register immediate code.
    /// </summary>
    [JsonPropertyName("rt_code")]
    public required RegImmFuncCode RtCode { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type
    {
        get
        {
            bool isBranch = RtCode is
                (>= RegImmFuncCode.BranchOnLessThanZero and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely) or
                (>= RegImmFuncCode.BranchOnLessThanZeroAndLink and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink);
            return isBranch ? MipsInstructionType.RegisterImmediateBranch : MipsInstructionType.RegisterImmediateTrap;
        }
    }
}
