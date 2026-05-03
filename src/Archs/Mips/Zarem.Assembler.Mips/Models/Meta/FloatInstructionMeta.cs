// Avishai Dernis 2026

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Functions.FloatProc;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing floating-point instructions.
/// </summary>
public record FloatInstructionMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction float function code.
    /// </summary>
    [JsonPropertyName("float_func")]
    public required FloatFuncCode Function { get; init; }

    /// <summary>
    /// Gets the instruction float function code.
    /// </summary>
    [JsonPropertyName("formats")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HashSet<MipsFloatFormat>? SupportedFormats { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type => MipsInstructionType.Float;
}
