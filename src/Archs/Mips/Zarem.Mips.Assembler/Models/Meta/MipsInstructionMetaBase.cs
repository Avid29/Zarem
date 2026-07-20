// Avishai Dernis 2026

using System.Text;
using System.Text.Json.Serialization;
using Zarem.Assembler;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Models.Tables;
using Zarem.Attributes.Arguments;
using Zarem.Mips.Extensions;
using Zarem.Mips.Models.Enums;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Assembler.Models.Meta;

/// <summary>
/// A base type for a MIPS instruction meta defintion.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RTypeInstructionMeta), "r-type")]
[JsonDerivedType(typeof(JTypeInstructionMeta), "j-type")]
[JsonDerivedType(typeof(ITypeInstructionMeta), "i-type")]
[JsonDerivedType(typeof(RegImmInstructionMeta), "reg_imm")]
[JsonDerivedType(typeof(CoProc0InstructionsMeta), "coproc0")]
[JsonDerivedType(typeof(CoProc1InstructionsMeta), "coproc1")]
[JsonDerivedType(typeof(MipsFloatInstructionMeta), "float")]
[JsonDerivedType(typeof(MipsPseudoInstructionMeta), "pseudo")]
public abstract record MipsInstructionMetaBase : InstructionMetaBase<MipsArgument>
{
    /// <summary>
    /// Gets the <see cref="MipsVersion"/> where the instruction was added.
    /// </summary>
    [JsonPropertyName("added_in")]
    public MipsVersion AddedIn { get; init; }

    /// <summary>
    /// Gets the <see cref="MipsVersion"/> where the instruction was removed, if applicable.
    /// </summary>
    [JsonPropertyName("removed_in")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MipsVersion? RemovedIn { get; init; }

    /// <summary>
    /// Gets whether or not the instruction only exists in 64-bit MIPS.
    /// </summary>
    [JsonPropertyName("64bit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Is64Bit { get; init; }

    /// <summary>
    /// Gets the fixed $rs value, if applicable.
    /// </summary>
    [JsonPropertyName("rs_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRS { get; init; }

    /// <summary>
    /// Gets the fixed $rt value, if applicable.
    /// </summary>
    [JsonPropertyName("rt_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRT { get; init; }

    /// <summary>
    /// Gets the fixed $rd value, if applicable.
    /// </summary>
    [JsonPropertyName("rd_fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? FixedRD { get; init; }

    /// <summary>
    /// Gets a string showing the usage pattern for the instruction.
    /// </summary>
    public string UsagePattern
    {
        get
        {
            StringBuilder pattern = new($"{Name} ");
            for (int i = 0; i < ArgumentPattern.Length; i++)
            {
                var profile = new MipsTokenizerProfile();
                pattern.Append(ArgumentTable<MipsArgument>.GetDisplay(ArgumentPattern[i], profile));

                if (i < ArgumentPattern.Length - 1)
                {
                    pattern.Append(", ");
                }
            }

            return $"{pattern}";
        }
    }

    /// <summary>
    /// Gets the function's type.
    /// </summary>
    [JsonIgnore]
    public abstract MipsInstructionType Type { get; }

    /// <summary>
    /// Check if an instruction is valid for a given version.
    /// </summary>
    public bool IsValidFor(MipsVersion version)
    {
        bool inRange = version >= AddedIn && !(RemovedIn.HasValue && version >= RemovedIn);
        bool sufficientRegisterSize = !Is64Bit || version.Is64Bit();
        return inRange && sufficientRegisterSize;
    }
}
