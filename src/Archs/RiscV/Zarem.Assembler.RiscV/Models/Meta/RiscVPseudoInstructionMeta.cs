// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing pseudo instructions.
/// </summary>
public record RiscVPseudoInstructionMeta : RiscVInstructionMetaBase, IPseudoInstructionMeta
{
    /// <summary>
    /// Gets the pseudo op for a pseudo-instruction.
    /// </summary>
    [JsonPropertyName("pseudo_id")]
    public required RiscVPseudoOp PseudoOp { get; init; }

    /// <summary>
    /// Gets the expansion of the pseudo-instruction into real instructions.
    /// </summary>
    [JsonPropertyName("expansion")]
    public required string[] Expansion { get; init; }
}
