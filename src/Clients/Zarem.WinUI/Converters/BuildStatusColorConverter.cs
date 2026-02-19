// Avishai Dernis 2025

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Zarem.Models.Enums;
using System;
using Windows.UI;

namespace Zarem.WinUI.Converters;

/// <summary>
/// A converter that converts an <see cref="BuildStatus"/> into a <see cref="Color"/>.
/// </summary>
public partial class BuildStatusColorConverter : DependencyObject, IValueConverter
{
    public static readonly DependencyProperty NotReadyProperty =
        DependencyProperty.Register(nameof(NotReady), typeof(Color), typeof(BuildStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty ReadyProperty =
        DependencyProperty.Register(nameof(Ready), typeof(Color), typeof(BuildStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty RunningProperty =
        DependencyProperty.Register(nameof(Running), typeof(Color), typeof(BuildStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty DoneProperty =
        DependencyProperty.Register(nameof(Done), typeof(Color), typeof(BuildStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty FailedProperty =
        DependencyProperty.Register(nameof(Failed), typeof(Color), typeof(BuildStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public Color NotReady
    {
        get => (Color)GetValue(NotReadyProperty);
        set => SetValue(NotReadyProperty, value);
    }

    public Color Ready
    {
        get => (Color)GetValue(ReadyProperty);
        set => SetValue(ReadyProperty, value);
    }

    public Color Running
    {
        get => (Color)GetValue(RunningProperty);
        set => SetValue(RunningProperty, value);
    }

    public Color Done
    {
        get => (Color)GetValue(DoneProperty);
        set => SetValue(DoneProperty, value);
    }

    public Color Failed
    {
        get => (Color)GetValue(FailedProperty);
        set => SetValue(FailedProperty, value);
    }

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value is null)
            return null;

        if (value is not BuildStatus type)
            throw new ArgumentException("Value must be of type InstructionType", nameof(value));

        return type switch
        {
            BuildStatus.NotReady => NotReady,
            BuildStatus.Ready => Ready,

            BuildStatus.Preparing or
            BuildStatus.Assembling or
            BuildStatus.Linking => Running,

            BuildStatus.Completed => Done,
            BuildStatus.Failed => Failed,

            _ => NotReady,
        };
    }
    
    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language) => null;
}
