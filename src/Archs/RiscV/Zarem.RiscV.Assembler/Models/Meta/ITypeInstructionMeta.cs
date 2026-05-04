// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;

namespace Zarem.RiscV.Assembler.Models.Meta;

/// <summary>
/// Instruction metadata for parsing RISC-V I-Type instructions.
/// </summary>
public record ITypeInstructionMeta : RiscVInstructionMetaBase
{
    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    public RiscVOpCode OpCode { get; init; } = RiscVOpCode.OpImmediate;

    /// <summary>
    /// Gets the instruction function3 code.
    /// </summary>
    [JsonPropertyName("funct3")]
    public Funct3Code Funct3 { get; init; }

    /// <summary>
    /// If true, the immediate is treated as a 5-bit SHAMT, 
    /// and the upper bits are set based on the specific shift type (Logical vs Arithmetic).
    /// </summary>
    [JsonPropertyName("is_shift")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsShift { get; init; }

    /// <summary>
    /// The specialized bits (0x00 or 0x20) to be ORed into the upper immediate 
    /// for shift instructions.
    /// </summary>
    [JsonPropertyName("special")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public byte Special { get; init; } = 0;
}
