// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing I-Type instructions.
/// </summary>
public record ITypeInstructionMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public required OperationCode OperationCode { get; init; }

    /// <inheritdoc/>
    public override InstructionType Type
    {
        get
        {
            bool isBranch = OperationCode is
                (>= OperationCode.BranchOnEquals and <= OperationCode.BranchOnGreaterThanZero) or
                (>= OperationCode.BranchOnEqualLikely and <= OperationCode.BranchOnGreaterThanZeroLikely);
            return isBranch ? InstructionType.IBranch : InstructionType.BasicI;
        }
    }
}
