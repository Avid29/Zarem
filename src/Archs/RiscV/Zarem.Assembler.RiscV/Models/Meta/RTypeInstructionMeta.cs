// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V R-Type instructions.
/// </summary>
public record RTypeInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public OperationCode OpCode { get; init; } = OperationCode.Alu;

    /// <summary>
    /// Gets the instruction function3 code.
    /// </summary>
    [JsonPropertyName("funct3")]
    public Funct3Code Funct3 { get; init; }

    /// <summary>
    /// Gets the instruction function7 code.
    /// </summary>
    [JsonPropertyName("funct7")]
    public Funct7Code Funct7 { get; init; } = Funct7Code.Base;
}
