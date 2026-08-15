// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.RiscV.Assembler.Models.Meta.Extensions.Compressed;

/// <summary>
/// Instruction metadata for parsing RISC-V CR-Type instructions.
/// </summary>
public record CRTypeInstructionMeta : CTypeInstructionMeta
{
    /// <summary>
    /// Gets the instruction's compressed function4 code.
    /// </summary>
    [JsonPropertyName("cfunct4")]
    public CFunct4Code CFunct4 { get; init; }
}
