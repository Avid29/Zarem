// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing R-Type instructions.
/// </summary>
public record RTypeInstructionMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public OperationCode OperationCode { get; init; } = OperationCode.Special;

    /// <summary>
    /// Gets the instruction function code.
    /// </summary>
    [JsonPropertyName("func_code")]
    public required FunctionCode FuncCode { get; init; }

    /// <inheritdoc/>
    public override InstructionType Type => OperationCode switch
    {
        OperationCode.Special2 => InstructionType.Special2R,
        OperationCode.Special3 => InstructionType.Special3R,
        OperationCode.Special or  _ => InstructionType.BasicR,
    };
}
