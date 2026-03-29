// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using System;

namespace Zarem.IDE.Controls.CodeEditor.Zarem;

public partial class ZaremCodeEditor
{
    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="Text"/> property.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(ZaremCodeEditor), new PropertyMetadata(string.Empty));

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="SelectedRange"/> property.
    /// </summary>
    public static readonly DependencyProperty SelectionRangeProperty =
        DependencyProperty.Register(nameof(SelectedRange), typeof(Range), typeof(ZaremCodeEditor), new PropertyMetadata(new Range(0, 0)));

    /// <summary>
    /// Gets or sets the text contained in the editbox.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set
        {
            if (Text == value)
                return;

            // Do not use the setter internally.
            // That causes unneccesary looping

            SetValue(TextProperty, value);
            Document.SetText(Microsoft.UI.Text.TextSetOptions.None, value);

            // TODO: Improve
            _ = UpdateSyntaxHighlightingAsync();
        }
    }

    /// <summary>
    /// Gets or sets the selected range.
    /// </summary>
    public Range SelectedRange
    {
        get => (Range)GetValue(SelectionRangeProperty);
        set => SetValue(SelectionRangeProperty, value);
    }

    private void UpdateTextProperty(string value) => SetValue(TextProperty, value);
}
