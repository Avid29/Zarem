// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;

namespace Zarem.Mips.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing R-Type instructions.
/// </summary>
public record RTypeInstructionMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public MipsOpCode OperationCode { get; init; } = MipsOpCode.Special;

    /// <summary>
    /// Gets the instruction function code.
    /// </summary>
    [JsonPropertyName("func_code")]
    public required FunctionCode FuncCode { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type => OperationCode switch
    {
        MipsOpCode.Special2 => MipsInstructionType.Special2R,
        MipsOpCode.Special3 => MipsInstructionType.Special3R,
        MipsOpCode.Special or  _ => MipsInstructionType.BasicR,
    };
}
