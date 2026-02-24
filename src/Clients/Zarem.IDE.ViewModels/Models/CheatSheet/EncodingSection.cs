// Avishai Dernis 2025

using Zarem.Models.CheatSheet.Enums;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Zarem.Models.CheatSheet;

/// <summary>
/// A class representing a section of encodings in the cheatsheet.
/// </summary>
public record EncodingSection
{
    /// <summary>
    /// Gets or sets the name of the encoding section.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the number of bits used for the section.
    /// </summary>
    [JsonPropertyName("size")]
    public int BitCount { get; set; }

    /// <summary>
    /// Gets or sets the starting bit offset for the section.
    /// </summary>
    [JsonPropertyName("offset")]
    public int BitOffset { get; set; }

    /// <summary>
    /// Gets or sets the constant value for the section, if applicable.
    /// </summary>
    [JsonPropertyName("value")]
    public int? ConstantValue { get; set; }

    /// <summary>
    /// Gets or sets the type of the section, indicating how it should be interpreted.
    /// </summary>
    [JsonPropertyName("type")]
    public EncodingSectionType? Type { get; set; }

    /// <summary>
    /// Gets how the section should be labelled in the encoding pattern display.
    /// </summary>
    public string? Display
    {
        get
        {
            if (!ConstantValue.HasValue)
                return Name;

            string binary = string.Format($"{{0:b{BitCount}}}", ConstantValue.Value);

            if (BitCount is 6)
            {
                return $"{binary[0..3]} {binary[3..6]}";
            }

            if (BitCount > 4)
            {
                string result = "";
                for (int i = 0; i < binary.Length; i += 4)
                {
                    var end = int.Min(i + 4, binary.Length);
                    result += $"{binary[i..end]} ";
                }

                return result.Trim();
            }

            return binary;
        }
    }

    /// <summary>
    /// Gets the start of the bit range for the section.
    /// </summary>
    public int BitRangeStart => BitOffset;

    /// <summary>
    /// Gets the end of the bit range for the section.
    /// </summary>
    public int BitRangeEnd => BitOffset + BitCount - 1;
}
