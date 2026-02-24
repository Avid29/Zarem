// Avishai Dernis 2025

using System.Text.Json.Serialization;

namespace Zarem.Models.CheatSheet;

/// <summary>
/// A class representing the detials of an instruction encoding type.
/// </summary>
public record EncodingPattern
{
    /// <summary>
    /// Gets or sets the name of the instruction encoding type.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the sections of data that make up the encoding format.
    /// </summary>
    [JsonPropertyName("sections")]
    public EncodingSection[]? Sections { get; set; }
}
