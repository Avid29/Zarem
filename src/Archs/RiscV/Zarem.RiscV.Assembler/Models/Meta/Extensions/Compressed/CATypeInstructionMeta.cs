// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.RiscV.Assembler.Models.Meta.Extensions.Compressed;

/// <summary>
/// Instruction metadata for parsing RISC-V CA-Type instructions.
/// </summary>
public record CATypeInstructionMeta : CTypeInstructionMeta
{
    /// <summary>
    /// Gets the instruction's compressed function2 code.
    /// </summary>
    [JsonPropertyName("cfunct2")]
    public CFunct2Code CFunct2 { get; init; }

    /// <summary>
    /// Gets the instruction's compressed function6 code.
    /// </summary>
    [JsonPropertyName("cfunct6")]
    public CFunct6Code CFunct6 { get; init; }
}
