// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.RiscV.Assembler.Models.Meta.Extensions.Compressed;

/// <summary>
/// Instruction metadata for parsing RISC-V CJ-Type instructions.
/// </summary>
public record CJTypeInstructionMeta : CTypeInstructionMeta
{
    /// <summary>
    /// Gets the instruction's compressed function3 code.
    /// </summary>
    [JsonPropertyName("cfunct3")]
    public CFunct3Code CFunct3 { get; init; }
}
