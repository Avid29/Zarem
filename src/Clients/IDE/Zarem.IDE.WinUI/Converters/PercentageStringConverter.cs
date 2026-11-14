// Avishai Dernis 2026

using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Zarem.IDE.Converters;

/// <summary>
/// An <see cref="IValueConverter"/> for converting zoom integers to string and vice-verse.
/// </summary>
public partial class PercentageStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int percent)
        {
            // The "P0" format with a 0.01 multiplier is the standard way 
            // to get localized percent strings (e.g., %120 in Turkish).
            return string.Format(CultureInfo.CurrentCulture, "{0:P0}", percent / 100.0);
        }

        return value?.ToString();
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        string? input = value as string;
        if (string.IsNullOrWhiteSpace(input))
            return 100;

        // Regex \d+ handles: "120%", "%120", "120 %", etc.
        string numericPart = Regex.Match(input, @"\d+").Value;

        if (int.TryParse(numericPart, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }
        return 100;
    }
}
