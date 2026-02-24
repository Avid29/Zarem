// Avishai Dernis 2025

using System.Text.Json.Serialization;

namespace Zarem.Models.EditorConfig.ColorScheme;

/// <summary>
/// A color scheme for editor syntax highlighting.
/// </summary>
public record SyntaxHighlightingScheme
{
    /// <summary>
    /// Gets the syntax highlighting color for instructions.
    /// </summary>
    [JsonPropertyName("instruction")]
    public required string Instruction { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for registers.
    /// </summary>
    [JsonPropertyName("register")]
    public required string Register { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for registers.
    /// </summary>
    [JsonPropertyName("immediate")]
    public required string Immediate { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for references.
    /// </summary>
    [JsonPropertyName("reference")]
    public required string Reference { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for operators.
    /// </summary>
    [JsonPropertyName("operator")]
    public required string Operator { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for operators.
    /// </summary>
    [JsonPropertyName("directive")]
    public required string Directive { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for operators.
    /// </summary>
    [JsonPropertyName("string")]
    public required string String { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for comments.
    /// </summary>
    [JsonPropertyName("comment")]
    public required string Comment { get; init; }

    /// <summary>
    /// Gets the syntax highlighting color for macros.
    /// </summary>
    [JsonPropertyName("macro")]
    public required string Macro { get; init; }
}
