// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing pseudo instructions.
/// </summary>
public record MipsPseudoInstructionMeta : MipsInstructionMetaBase, IPseudoInstructionMeta
{
    /// <inheritdoc/>
    [JsonPropertyName("expansion")]
    public required string[] Expansion { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type => MipsInstructionType.Pseudo;
}
