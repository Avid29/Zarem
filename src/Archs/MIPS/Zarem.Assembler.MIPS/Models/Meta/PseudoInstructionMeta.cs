// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing pseudo instructions.
/// </summary>
public record PseudoInstructionMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the pseudo op for a pseudo-instruction.
    /// </summary>
    [JsonPropertyName("pseudo_id")]
    public required PseudoOp PseudoOp { get; init; }

    /// <summary>
    /// Gets the number of real instructions required to execute the instruction.
    /// </summary>
    /// <remarks>
    /// This exists for pseudo instructions.
    /// </remarks>
    [JsonPropertyName("expansion_count")]
    public required int? RealizedCount { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type => MipsInstructionType.Pseudo;
}
