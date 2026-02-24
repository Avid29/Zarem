// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;

namespace Zarem.IDE.Controls.HexViewer;

/// <summary>
/// A hex viewer.
/// </summary>
[TemplatePart(Name = HexGridPartName, Type = typeof(GridView))]
[TemplatePart(Name = CharGridPartName, Type = typeof(GridView))]
public sealed partial class HexViewer : Control
{
    private GridView? _hexGrid;
    private GridView? _charGrid;

    private const string HexGridPartName = "HexGrid";
    private const string CharGridPartName = "CharGrid";

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="Content"/> property.
    /// </summary>
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(string), typeof(HexViewer), new PropertyMetadata(string.Empty, OnContentPropertyChanged));

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="Data"/> property.
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(byte[]), typeof(HexViewer), new PropertyMetadata(Array.Empty<byte>()));
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HexViewer"/> class.
    /// </summary>
    public HexViewer()
    {
        DefaultStyleKey = typeof(HexViewer);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        _hexGrid = GetTemplateChild(HexGridPartName) as GridView;
        _charGrid = GetTemplateChild(CharGridPartName) as GridView;
    }

    /// <summary>
    /// Gets or sets the byte data to display.
    /// </summary>
    public string Content
    {
        get => (string)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>
    /// Gets or sets the byte data to display.
    /// </summary>
    public byte[] Data
    {
        get => (byte[])GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HexViewer viewer)
            return;

        viewer.Data = Encoding.Default.GetBytes(viewer.Content);
    }
}
