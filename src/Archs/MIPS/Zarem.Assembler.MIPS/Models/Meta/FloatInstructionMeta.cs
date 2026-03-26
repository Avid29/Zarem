// Avishai Dernis 2026

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Helpers.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

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
    public HashSet<FloatFormat>? SupportedFormats { get; init; }

    /// <inheritdoc/>
    public override InstructionType Type => InstructionType.Float;
}
