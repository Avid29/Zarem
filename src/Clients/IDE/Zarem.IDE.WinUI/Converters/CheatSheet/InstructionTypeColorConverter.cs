// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Zarem.Mips.Models.Instructions.Enums;

namespace Zarem.IDE.Converters;

/// <summary>
/// A converter that converts an <see cref="MipsInstructionType"/> into a <see cref="SolidColorBrush"/>.
/// </summary>
public partial class InstructionTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value is null)
            return null;

        if (value is not MipsInstructionType type)
            throw new ArgumentException("Value must be of type InstructionType", nameof(value));

        return type switch
        {
            MipsInstructionType.BasicR => RTypeInstructionColor,
            MipsInstructionType.BasicI => ITypeInstructionColor,
            MipsInstructionType.BasicJ => JTypeInstructionColor,

            MipsInstructionType.RegisterImmediateTrap or
            MipsInstructionType.RegisterImmediateBranch => RegImmInstructionColor,

            MipsInstructionType.Special2R or
            MipsInstructionType.Special3R => SpecialInstructionColor,

            MipsInstructionType.Coproc0 => CoProc0InstructionColor,

            MipsInstructionType.Float or
            MipsInstructionType.Coproc1 => CoProc1InstructionColor,

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<object>($"Unknown InstructionType: {type}"),
        };
    }
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }

    /// <summary>
    /// Gets or sets the color for R-type instructions.
    /// </summary>
    public Brush? RTypeInstructionColor { get; set; }
    
    /// <summary>
    /// Gets or sets the color for I-type instructions.
    /// </summary>
    public Brush? ITypeInstructionColor { get; set; }

    /// <summary>
    /// Gets or sets the color for J-type instructions.
    /// </summary>
    public Brush? JTypeInstructionColor { get; set; }

    /// <summary>
    /// Gets or sets the color for register immediate type instructions.
    /// </summary>
    public Brush? RegImmInstructionColor { get; set; }

    /// <summary>
    /// Gets or sets the color for special2/3 instructions.
    /// </summary>
    public Brush? SpecialInstructionColor { get; set; }

    /// <summary>
    /// Gets or sets the color for coproc0 instructions.
    /// </summary>
    public Brush? CoProc0InstructionColor { get; set; }

    /// <summary>
    /// Gets or sets the color for coproc1 instructions.
    /// </summary>
    public Brush? CoProc1InstructionColor { get; set; }
}
