// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Operations;

namespace Zarem.RiscV.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V J-Type instructions.
/// </summary>
public record JTypeInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public RiscVOpCode OpCode { get; init; } = RiscVOpCode.JumpAndLink;
}
