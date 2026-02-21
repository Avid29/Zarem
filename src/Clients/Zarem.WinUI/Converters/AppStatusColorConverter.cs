// Avishai Dernis 2025

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Zarem.Models.Enums;
using System;
using Windows.UI;

namespace Zarem.WinUI.Converters;

/// <summary>
/// A converter that converts an <see cref="IdeState"/> into a <see cref="Color"/>.
/// </summary>
public partial class AppStatusColorConverter : DependencyObject, IValueConverter
{
    public static readonly DependencyProperty NotReadyProperty =
        DependencyProperty.Register(nameof(NotReady), typeof(Color), typeof(AppStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty ReadyProperty =
        DependencyProperty.Register(nameof(Ready), typeof(Color), typeof(AppStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty BuildingProperty =
        DependencyProperty.Register(nameof(Building), typeof(Color), typeof(AppStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty DoneProperty =
        DependencyProperty.Register(nameof(Done), typeof(Color), typeof(AppStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty FailedProperty =
        DependencyProperty.Register(nameof(Failed), typeof(Color), typeof(AppStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
    public static readonly DependencyProperty RunningProperty =
        DependencyProperty.Register(nameof(Running), typeof(Color), typeof(AppStatusColorConverter), new PropertyMetadata(Colors.Transparent));
    
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

    public Color Building
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

    public Color Running
    {
        get => (Color)GetValue(RunningProperty);
        set => SetValue(RunningProperty, value);
    }

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value is null)
            return null;

        if (value is not IdeState type)
            throw new ArgumentException("Value must be of type InstructionType", nameof(value));

        return type switch
        {
            IdeState.NotReady => NotReady,
            IdeState.Ready => Ready,
            IdeState.Building => Building,
            IdeState.BuildComplete => Done,
            IdeState.Failed => Failed,
            IdeState.Runnning => Running,

            _ => NotReady,
        };
    }
    
    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language) => null;
}
