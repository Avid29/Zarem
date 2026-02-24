// Avishai Dernis 2025

using System.Text.Json.Serialization;

namespace Zarem.Models.CheatSheet;

/// <summary>
/// A class representing a group of instructions.
/// </summary>
public class InstructionGroup
{
    /// <summary>
    /// Gets or sets the name of the instruction group.
    /// </summary>
    [JsonPropertyName("name")]
    public required string GroupName { get; set; }

    /// <summary>
    /// Gets or sets the instructions in the group.
    /// </summary>
    [JsonPropertyName("instructions")]
    public required string[] Instructions { get; set; }
}
