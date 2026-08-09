// Avishai Dernis 2026

using System.Text.Json.Serialization;
using Zarem.Assembler;
using Zarem.Assembler.Models.Meta;
using Zarem.Models.Versioning;
using Zarem.RiscV.Assembler.Models.Meta.Extensions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Assembler.Models.Meta;

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
[JsonDerivedType(typeof(RiscVFloatInstructionMeta), "float")]
[JsonDerivedType(typeof(CITypeInstructionMeta), "ci-type")]
[JsonDerivedType(typeof(RiscVPseudoInstructionMeta), "pseudo")]
public abstract record RiscVInstructionMetaBase : InstructionMetaBase<RiscVArgument>
{
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
    /// Gets the fixed rd value, if applicable.
    /// </summary>
    [JsonPropertyName("rd_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRD { get; init; }

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
    /// Gets the fixed rs3 value, if applicable.
    /// </summary>
    [JsonPropertyName("rs3_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRS3 { get; init; }

    /// <summary>
    /// Gets the fixed immediate value, if applicable.
    /// </summary>
    [JsonPropertyName("imm_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FixedImm { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string UsagePattern => GetUsagePattern(RiscVTokenizerProfile.Default);

    /// <summary>
    /// Checks if an instruction is valid for the provided processor configuration.
    /// </summary>
    public bool IsValidFor(RiscVVersionInfo config)
    {
        // Check if the required extension is enabled in the flags
        if (!config.Extensions.HasFlag(Extension))
            return false;

        // Check if the current CPU base width meets the minimum requirement
        if ((int)config.Base < (int)MinBase)
            return false;

        // Check spec version
        return config.SpecMajor >= (int)Version;
    }
}
