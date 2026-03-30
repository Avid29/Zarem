// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.Assembler.Models;

/// <summary>
/// A base type for an instruction meta defintion.
/// </summary>
public abstract record InstructionMetaBase
{
    /// <summary>
    /// Gets the name of the instruction.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets a string describing the behavior of the instruction.
    /// </summary>
    [JsonPropertyName("behavior")]
    public string? Behavior { get; init; }

    /// <summary>
    /// Gets the number of argument required by the instruction.
    /// </summary>
    [JsonIgnore]
    public abstract int ArgumentCount { get; }
}
