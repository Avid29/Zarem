// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.RiscV.Assembler.Models.Meta.Extensions.Compressed;

/// <summary>
/// Instruction metadata for parsing RISC-V CB-Type instructions with arithmetic behavior.
/// </summary>
public record CBATypeInstructionMeta : CBTypeInstructionMeta
{
    /// <summary>
    /// Gets the instruction's compressed function2 code.
    /// </summary>
    [JsonPropertyName("cfunct2")]
    public CFunct2Code CFunct2 { get; init; }
}
