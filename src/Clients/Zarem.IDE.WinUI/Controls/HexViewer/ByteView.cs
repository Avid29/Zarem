// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Zarem.IDE.Controls.HexViewer.Enums;

namespace Zarem.IDE.Controls.HexViewer;

/// <summary>
/// A control that displays a single byte
/// </summary>
public sealed partial class ByteView : Control
{
    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="Data"/> property.
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(byte), typeof(ByteView), new PropertyMetadata((byte)0, OnPropertyChanged));
    
    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="DisplayMode"/> property.
    /// </summary>
    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(ByteDisplayMode), typeof(ByteView), new PropertyMetadata(ByteDisplayMode.Hex, OnPropertyChanged));
    
    private static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(ByteView), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteView"/> class.
    /// </summary>
    public ByteView()
    {
        DefaultStyleKey = typeof(ByteView);

        this.DataContextChanged += ByteView_DataContextChanged;
    }

    /// <summary>
    /// Gets or sets the byte data to display.
    /// </summary>
    public byte Data
    {
        get => (byte)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the way to display the byte data.
    /// </summary>
    public ByteDisplayMode DisplayMode
    {
        get => (ByteDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    private string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    private void ByteView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (sender is not ByteView view)
            return;

        if (args.NewValue is not char c)
            return;

        view.Data = BitConverter.GetBytes(c)[0];
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ByteView view)
            return;

        byte data = view.Data;
        view.DisplayText = view.DisplayMode switch
        {
            ByteDisplayMode.Hex => Convert.ToString(data, 16),
            ByteDisplayMode.Ascii or 
            _ => $"{(char)data}",
        };
    }
}
