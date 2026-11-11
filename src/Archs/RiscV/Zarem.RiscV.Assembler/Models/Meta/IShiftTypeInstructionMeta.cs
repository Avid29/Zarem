// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.RiscV.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V I-Type instructions.
/// </summary>
public record IShiftTypeInstructionMeta : ITypeInstructionMeta
{
    /// <summary>
    /// The specialized bits (0x00 or 0x20) to be ORed into the upper immediate 
    /// for shift instructions.
    /// </summary>
    [JsonPropertyName("special")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public byte Special { get; init; } = 0;
}
