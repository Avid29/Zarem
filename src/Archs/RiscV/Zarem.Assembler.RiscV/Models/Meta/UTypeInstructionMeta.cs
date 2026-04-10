// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V U-Type instructions.
/// </summary>
public record UTypeInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public RiscVOpCode OpCode { get; init; } = RiscVOpCode.LoadUpperImmediate;

    /// <summary>
    /// Gets the instruction function3 code.
    /// </summary>
    [JsonPropertyName("funct3")]
    public Funct3Code Funct3 { get; init; }
}
