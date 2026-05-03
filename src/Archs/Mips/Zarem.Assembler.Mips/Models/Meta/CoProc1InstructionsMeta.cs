// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Functions.FloatProc;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing coprocessor0 instructions.
/// </summary>
public record CoProc1InstructionsMeta : MipsInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction rs function code for a coproc1 instruction.
    /// </summary>
    [JsonPropertyName("rs_code")]
    public required CoProc1RSCode RSCode { get; init; }

    /// <inheritdoc/>
    public override MipsInstructionType Type => MipsInstructionType.Coproc1;
}
