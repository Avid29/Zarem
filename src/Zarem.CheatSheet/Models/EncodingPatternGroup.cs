// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.CheatSheet.Models;

/// <summary>
/// A class representing a group of encoding patterns.
/// </summary>
public record EncodingPatternGroup
{
    /// <summary>
    /// Gets or sets the name of the group.
    /// </summary>
    [JsonPropertyName("name")]
    public required string? Name { get; init; }

    /// <summary>
    /// Gets or sets the the encoding .
    /// </summary>
    [JsonPropertyName("patterns")]
    public required EncodingPattern[] Patterns { get; init; }
}
