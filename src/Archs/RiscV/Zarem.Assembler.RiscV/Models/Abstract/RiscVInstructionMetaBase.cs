// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Meta;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Assembler.Models.Abstract;

/// <summary>
/// A base type for a RISC-V instruction meta definition.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RTypeInstructionMeta), "r-type")]
[JsonDerivedType(typeof(ITypeInstructionMeta), "i-type")]
[JsonDerivedType(typeof(STypeInstructionMeta), "s-type")]
[JsonDerivedType(typeof(BTypeInstructionMeta), "b-type")]
[JsonDerivedType(typeof(UTypeInstructionMeta), "u-type")]
[JsonDerivedType(typeof(JTypeInstructionMeta), "j-type")]
//[JsonDerivedType(typeof(PseudoInstructionMeta), "pseudo")]
public abstract record RiscVInstructionMetaBase : InstructionMetaBase
{
    /// <summary>
    /// Gets the instruction argument pattern for parsing.
    /// </summary>
    [JsonPropertyName("args")]
    public required Argument[] ArgumentPattern { get; init; }

    /// <inheritdoc/>
    public override int ArgumentCount =>  ArgumentPattern.Length;

    /// <summary>
    /// Gets the extension required to execute this instruction (e.g., M, A, F).
    /// </summary>
    [JsonPropertyName("extension")]
    public RiscVExtensions Extension { get; init; } = RiscVExtensions.Integers;

    /// <summary>
    /// Gets the minimum base architecture width (32, 64, or 128).
    /// </summary>
    [JsonPropertyName("min_base")]
    public RiscVBaseVersion MinBase { get; init; } = RiscVBaseVersion.RV32;

    /// <summary>
    /// Gets the specific version of the extension this instruction was introduced in.
    /// </summary>
    [JsonPropertyName("version")]
    public double Version { get; init; } = 2.0;

    /// <summary>
    /// Gets the fixed rs1 value, if applicable.
    /// </summary>
    [JsonPropertyName("rs1_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRS1 { get; init; }

    /// <summary>
    /// Gets the fixed rs2 value, if applicable.
    /// </summary>
    [JsonPropertyName("rs2_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRS2 { get; init; }

    /// <summary>
    /// Gets the fixed rd value, if applicable.
    /// </summary>
    [JsonPropertyName("rd_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRD { get; init; }

    /// <summary>
    /// Checks if an instruction is valid for the provided processor configuration.
    /// </summary>
    public bool IsValidFor(RiscVVersionInfo config)
    {
        // 1. Check if the required extension is enabled in the flags
        if (!config.Extensions.HasFlag(Extension))
            return false;

        // 2. Check if the current CPU base width meets the minimum requirement
        if ((int)config.Base < (int)MinBase)
            return false;

        // 3. Optional: Check spec version if you're supporting multiple drafts
        return config.SpecMajor >= (int)Version;
    }
}
