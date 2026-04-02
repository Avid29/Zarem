// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Assembler.Models.Meta;

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
    public override InstructionType Type
    {
        get
        {
            bool isBranch = RtCode is
                (>= RegImmFuncCode.BranchOnLessThanZero and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely) or
                (>= RegImmFuncCode.BranchOnLessThanZeroAndLink and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink);
            return isBranch ? InstructionType.RegisterImmediateBranch : InstructionType.RegisterImmediateTrap;
        }
    }
}
