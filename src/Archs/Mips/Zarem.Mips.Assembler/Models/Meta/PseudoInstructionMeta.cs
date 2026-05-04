// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Models.Instructions.Enums;

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
