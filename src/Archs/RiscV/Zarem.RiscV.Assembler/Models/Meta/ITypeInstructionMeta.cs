// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;

namespace Zarem.RiscV.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V I-Type instructions.
/// </summary>
public record ITypeInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public RiscVOpCode OpCode { get; init; } = RiscVOpCode.OpImmediate;

    /// <summary>
    /// Gets the instruction function3 code.
    /// </summary>
    [JsonPropertyName("funct3")]
    public Funct3Code Funct3 { get; init; }
}
