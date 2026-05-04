// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;

namespace Zarem.RiscV.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V floating-Point instructions.
/// </summary>
public record RiscVFloatInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction's operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public RiscVOpCode OpCode { get; init; } = RiscVOpCode.FloatCompute;

    /// <summary>
    /// Gets the instruction's funct5 code.
    /// </summary>
    [JsonPropertyName("funct5")]
    public FloatFunc5Code? Funct5 { get; init; } = null;

    /// <summary>
    /// Gets the instruction's funct3 code.
    /// </summary>
    [JsonPropertyName("funct3")]
    public FloatFunct3Code Funct3 { get; init; } = FloatFunct3Code.RoundToNearest;
}
