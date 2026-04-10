// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Assembler.Helpers.Tables;

/// <summary>
/// A class containing methods for floating-point formats lookups.
/// </summary>
public static class FloatFormatTable
{
    /// <summary>
    /// Attempts to get a float format by name.
    /// </summary>
    public static bool TryGetFloatFormat(string name, out MipsFloatFormat format, out string lookupName)
    {
        format = 0;
        lookupName = string.Empty;

        // Split the text at the last period.
        // If there is no period, then the string does not contain a float format.
        var split = name.LastIndexOf('.');
        if (split is -1)
            return false;

        // Generate a lookup name by replacing the last part of the name with ".fmt"
        // and determine the float format based on the last part of the name.
        lookupName = name[..split] + ".fmt";
        format = name[(split+1)..].ToLower() switch
        {
            "s" => MipsFloatFormat.Single,
            "d" => MipsFloatFormat.Double,
            "l" => MipsFloatFormat.Long,
            "w" => MipsFloatFormat.Word,
            "ps" => MipsFloatFormat.PairedSingle,
            _ => 0
        };

        return format is not 0;
    }
    
    /// <summary>
    /// Attempts to get a float-format string by value.
    /// </summary>
    public static string GetFloatFormatString(MipsFloatFormat format)
    {
        return format switch
        {
            MipsFloatFormat.Single => "S",
            MipsFloatFormat.Double => "D",
            MipsFloatFormat.Word => "W",
            MipsFloatFormat.Long => "L",
            MipsFloatFormat.PairedSingle => "PS",
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>("Invalid float format"),
        };
    }

    /// <summary>
    /// Replaces the ".fmt" in a name with the appropriate float format string.
    /// </summary>
    public static string ApplyFormat(string name, MipsFloatFormat format)
    {
        return name.Replace(".fmt", $".{GetFloatFormatString(format)}");
    }
}
