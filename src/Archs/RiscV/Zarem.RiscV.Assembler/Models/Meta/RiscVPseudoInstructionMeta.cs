// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing pseudo instructions.
/// </summary>
public record RiscVPseudoInstructionMeta : RiscVInstructionMetaBase, IPseudoInstructionMeta
{
    /// <summary>
    /// Gets the expansion of the pseudo-instruction into real instructions.
    /// </summary>
    [JsonPropertyName("expansion")]
    public required string[] Expansion { get; init; }
}
