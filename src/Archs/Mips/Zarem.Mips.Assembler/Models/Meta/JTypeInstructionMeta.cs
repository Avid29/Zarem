// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Operations;

namespace Zarem.Mips.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing J-Type instructions.
/// </summary>
public record JTypeInstructionMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public required MipsOpCode OperationCode { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type => MipsInstructionType.BasicJ;
}
