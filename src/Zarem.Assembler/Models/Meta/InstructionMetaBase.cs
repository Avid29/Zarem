// Avishai Dernis 2026

using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Zarem.Assembler.Models.Tables;
using Zarem.Assembler.Tokenization.Profiles;

namespace Zarem.Assembler.Models.Meta;

/// <summary>
/// A base type for an instruction meta defintion.
/// </summary>
[DebuggerDisplay("{UsagePattern}")]
public abstract record InstructionMetaBase<TArg> : IInstructionMeta
    where TArg : unmanaged, Enum
{
    /// <inheritdoc/>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets a string describing the behavior of the instruction.
    /// </summary>
    [JsonPropertyName("behavior")]
    public string? Behavior { get; init; }

    /// <summary>
    /// Gets the instruction argument pattern for parsing.
    /// </summary>
    [JsonPropertyName("args")]
    public required TArg[] ArgumentPattern { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public string Identifier => $"{Name}:{ArgumentPattern.Length}";

    /// <inheritdoc/>
    [JsonIgnore]
    public int ArgumentCount => ArgumentPattern.Length;

    /// <inheritdoc/>
    [JsonIgnore]
    public abstract string UsagePattern { get; }

    /// <summary>
    /// Gets a string showing the usage pattern for the instruction, using the provided tokenizer profile.
    /// </summary>
    public string GetUsagePattern(ITokenizerProfile profile)
    {
        StringBuilder pattern = new($"{Name} ");
        for (int i = 0; i < ArgumentPattern.Length; i++)
        {
            pattern.Append(ArgumentTable<TArg>.GetDisplay(ArgumentPattern[i], profile));
            if (i < ArgumentPattern.Length - 1)
            {
                pattern.Append(", ");
            }
        }

        return $"{pattern}";
    }
}
