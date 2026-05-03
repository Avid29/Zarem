// Avishai Dernis 2026

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Functions;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V floating-Point instructions.
/// </summary>
public record FloatInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction float function code.
    /// </summary>
    [JsonPropertyName("float_func")]
    public required RiscVFloatFuncCode Function { get; init; }

    /// <summary>
    /// Gets the instruction float function code.
    /// </summary>
    [JsonPropertyName("formats")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HashSet<RiscVFloatFormat>? SupportedFormats { get; init; }
}
