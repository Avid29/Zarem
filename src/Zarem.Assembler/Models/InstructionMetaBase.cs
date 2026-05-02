// Avishai Dernis 2026

using System;
using System.Text.Json.Serialization;

namespace Zarem.Assembler.Models;

/// <summary>
/// A base type for an instruction meta defintion.
/// </summary>
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
    public int ArgumentCount => ArgumentPattern.Length;
}
