// Avishai Dernis 2025

using Zarem.Services.Settings.Enums;
using System.Text.Json.Serialization;

namespace Zarem.Models.EditorConfig.ColorScheme;

/// <summary>
/// A color scheme for the code editor.
/// </summary>
public record EditorColorScheme
{
    /// <summary>
    /// Gets the name of the color scheme.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the theme of the color scheme.
    /// </summary>
    [JsonPropertyName("theme")]
    public required Theme Theme { get; set; }

    /// <summary>
    /// Gets the foreground color of the color scheme.
    /// </summary>
    [JsonPropertyName("foreground")]
    public required string Foreground { get; init; }

    /// <summary>
    /// Gets the background color of the color scheme.
    /// </summary>
    [JsonPropertyName("background")]
    public required string Background { get; init; }

    /// <summary>
    /// Gets the syntax highlighting colors of the color scheme.
    /// </summary>
    [JsonPropertyName("syntax_highlighting")]
    public required SyntaxHighlightingScheme SyntaxHighlighting { get; init; }

    /// <summary>
    /// Gets the log highlighting colors of the color scheme.
    /// </summary>
    [JsonPropertyName("log_colors")]
    public required LogHighlightScheme LogColors { get; init; }
}
