// Avishai Dernis 2025

using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Zarem.IDE.Controls.CheatSheet.Palettes;
using Zarem.Models.Instructions.Enums;
using Zarem.IDE.Models.CheatSheet.Enums;

namespace Zarem.IDE.Converters;

/// <summary>
/// A converter that converts an <see cref="InstructionType"/> into a <see cref="SolidColorBrush"/>.
/// </summary>
public partial class EncodingPatternSectionColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value is null || Palette is null)
            return null;

        if (value is not EncodingSectionType type)
            throw new ArgumentException("Value must be of type InstructionType", nameof(value));

        return type switch
        {
            EncodingSectionType.OpCode => Palette.OpCodeBrush,
            EncodingSectionType.FunctionCode => Palette.FuncCodeBrush,
            EncodingSectionType.RegisterFuncCode => Palette.RegisterFuncCodeBrush,
            EncodingSectionType.Immediate => Palette.ImmediateValueBrush,
            EncodingSectionType.GeneralPurposeRegister => Palette.GPRegisterBrush,
            EncodingSectionType.CoProcessorRegister => Palette.CPRegisterBrush,
            EncodingSectionType.Other => Palette.FormatBrush,
            EncodingSectionType.GeneralOrCoProcessorRegister => Palette.GeneralOrCoRegisterBrush,
            _ or EncodingSectionType.Reserved => new SolidColorBrush(Colors.Transparent),
            //_ => ThrowHelper.ThrowArgumentOutOfRangeException<object>($"Unknown InstructionType: {type}"),
        };
    }
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }

    public EncodingPatternSectionBrushPalette? Palette { get; set; }
}
