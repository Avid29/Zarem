// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing coprocessor0 instructions.
/// </summary>
public record CoProc0InstructionsMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction rs function code for a coproc0 instruction.
    /// </summary>
    [JsonPropertyName("rs_code")]
    public required CoProc0RSCode RSCode { get; init; }

    /// <summary>
    /// Gets the instruction coprocessor0 function code.
    /// </summary>
    [JsonPropertyName("func_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Co0FuncCode? FuncCode { get; init; }

    /// <summary>
    /// Gets the instruction mfmc0 function code.
    /// </summary>
    [JsonPropertyName("mfmc0_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MFMC0FuncCode? Mfmc0FuncCode { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type => MipsInstructionType.Coproc0;
}
