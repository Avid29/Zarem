// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Operations;

namespace Zarem.RiscV.Assembler.Models.Meta.Extensions.Compressed;

/// <summary>
/// Instruction metadata for parsing RISC-V C-Type instructions.
/// </summary>
public abstract record CTypeInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("comp_code")]
    public RiscVCompressionCode CompressionCode { get; init; }
}
