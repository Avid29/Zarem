// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.RiscV.Assembler.Models.Meta.Extensions.Compressed;

/// <summary>
/// A base record for an instruction metadata for parsing a compressed RISC-V instruction with a funct3 code.
/// </summary>
public abstract record CFunct3InstructionMetaBase : CTypeInstructionMeta
{
    /// <summary>
    /// Gets the instruction's compressed function3 code.
    /// </summary>
    [JsonPropertyName("cfunct3")]
    public CFunct3Code CFunct3 { get; init; }
}
