// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V floating-Point instructions.
/// </summary>
public record RiscVFloatInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public RiscVOpCode OpCode { get; init; } = RiscVOpCode.OpImmediate;

    /// <summary>
    /// Gets the instruction float function code.
    /// </summary>
    [JsonPropertyName("float_func")]
    public required RiscVFloatFuncCode Function { get; init; }
}
