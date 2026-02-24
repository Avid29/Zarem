// Avishai Dernis 2025

using System.Text.Json.Serialization;

namespace Zarem.Models.EditorConfig.ColorScheme;

/// <summary>
/// A color scheme for editor log highlighting.
/// </summary>
public record LogHighlightScheme
{
    /// <summary>
    /// Gets the error underline/annotation color.
    /// </summary>
    [JsonPropertyName("error")]
    public required string Error { get; init; }

    /// <summary>
    /// Gets the warning underline/annotation color.
    /// </summary>
    [JsonPropertyName("warning")]
    public required string Warning { get; init; }

    /// <summary>
    /// Gets the message underline/annotation color.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
