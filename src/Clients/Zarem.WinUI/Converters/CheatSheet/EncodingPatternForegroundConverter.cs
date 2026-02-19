// Avishai Dernis 2025

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Zarem.Models.CheatSheet.Enums;
using System;
using Zarem.Models.Instructions.Enums;

namespace Zarem.WinUI.Converters;

/// <summary>
/// A converter that converts an <see cref="InstructionType"/> into a <see cref="SolidColorBrush"/>.
/// </summary>
public partial class EncodingPatternForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value is null || Default is null || Reserved is null)
            return null;

        if (value is not EncodingSectionType type)
            throw new ArgumentException("Value must be of type InstructionType", nameof(value));

        return type switch
        {
            EncodingSectionType.Reserved => Reserved,
            _ => Default,
        };
    }
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }

    public SolidColorBrush? Default { get; set; }

    public SolidColorBrush? Reserved { get; set; }
}
