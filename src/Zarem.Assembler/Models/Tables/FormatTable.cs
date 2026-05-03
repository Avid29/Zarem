// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Zarem.Assembler.Models.Tables;

/// <summary>
/// A class for managing format lookup tables.
/// </summary>
public class FormatTable<TFormat>
    where TFormat : unmanaged, Enum
{
    private readonly Dictionary<string, TFormat> _nameToFormat;
    private readonly Dictionary<TFormat, string> _formatToName;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FormatTable{TFormat}"/> class.
    /// </summary>
    public FormatTable(string placeholder = "fmt")
    {
        Placeholder = placeholder;
        _nameToFormat = new(StringComparer.OrdinalIgnoreCase);
        _formatToName = [];

        foreach (var field in typeof(TFormat).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            if (attr != null && field.GetValue(null) is TFormat value)
            {
                _nameToFormat[attr.Name] = value;
                _formatToName[value] = attr.Name;
            }
        }
    }

    /// <summary>
    /// Gets the format placeholder.
    /// </summary>
    public string Placeholder { get; }

    /// <summary>
    /// Attempts to get a float format by name.
    /// </summary>
    public bool TryGetFormat(string part, out TFormat format) => _nameToFormat.TryGetValue(part, out format);

    /// <summary>
    /// Attempts to get a float-format string by value.
    /// </summary>
    public string GetFormatString(TFormat format) => _formatToName.GetValueOrDefault(format, "??");

    /// <summary>
    /// Applies the format to a format string by replacing the placeholder with the corresponding format string.
    /// </summary>
    public string ApplyFormat(string formatString, TFormat format) => formatString.Replace(Placeholder, GetFormatString(format));
}
