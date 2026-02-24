// Avishai Dernis 2025

using System.Text.Json.Serialization;

namespace Zarem.Models.CheatSheet;

/// <summary>
/// A class representing a collection of groups of instructions.
/// </summary>
public class InstructionCollection
{
    /// <summary>
    /// Gets or sets the name of the instruction.
    /// </summary>
    [JsonPropertyName("name")]
    public required string CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the groups of instruction in the collection.
    /// </summary>
    [JsonPropertyName("groups")]
    public required InstructionGroup[] Groups { get; set; }
}
