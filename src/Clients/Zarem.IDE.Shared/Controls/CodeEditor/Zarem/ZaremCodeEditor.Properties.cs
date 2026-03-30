// Avishai Dernis 2026

using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor.Zarem;

public partial class ZaremCodeEditor
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(ZaremCodeEditor), new PropertyMetadata(string.Empty, OnTextPropertyChanged));

    public static readonly DependencyProperty LineProperty =
        DependencyProperty.Register(nameof(Line), typeof(long), typeof(ZaremCodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register(nameof(Column), typeof(long), typeof(ZaremCodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(int), typeof(ZaremCodeEditor), new PropertyMetadata(100, OnZoomPropertyChanged));

    public static readonly DependencyProperty ColorSchemeProperty =
        DependencyProperty.Register(nameof(ColorScheme), typeof(AssemblySyntaxColorScheme), typeof(ZaremCodeEditor), new PropertyMetadata(null, OnColorSchemePropertyChanged));

    public static readonly DependencyProperty AnnotationThresholdProperty =
        DependencyProperty.Register(nameof(AnnotationThreshold), typeof(AnnotationThreshold), typeof(ZaremCodeEditor), new PropertyMetadata(AnnotationThreshold.Errors, OnLogAnnotationsChanged));

    public static readonly DependencyProperty ExecutingLocationProperty =
        DependencyProperty.Register(nameof(ExecutingLocation), typeof(SourceRange?), typeof(ZaremCodeEditor), new PropertyMetadata(null));

    /// <inheritdoc/>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <inheritdoc/>
    public long Line
    {
        get => (long)GetValue(LineProperty);
        set => SetValue(LineProperty, value);
    }

    /// <inheritdoc/>
    public long Column
    {
        get => (long)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    /// <inheritdoc/>
    public int Zoom
    {
        get => (int)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <inheritdoc/>
    public AssemblySyntaxColorScheme? ColorScheme
    {
        get => (AssemblySyntaxColorScheme?)GetValue(ColorSchemeProperty);
        set => SetValue(ColorSchemeProperty, value);
    }

    /// <inheritdoc/>
    public AnnotationThreshold AnnotationThreshold
    {
        get => (AnnotationThreshold)GetValue(AnnotationThresholdProperty);
        set => SetValue(AnnotationThresholdProperty, value);
    }

    public SourceRange? ExecutingLocation
    {
        get => (SourceRange?)GetValue(ExecutingLocationProperty);
        set => SetValue(ExecutingLocationProperty, value);
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not ZaremCodeEditor codeEditor)
            return;

        codeEditor.UpdateText();
    }

    private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not ZaremCodeEditor codeEditor)
            return;

        //codeEditor.UpdatePosition();
    }

    private static void OnZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not ZaremCodeEditor codeEditor)
            return;

        //codeEditor.UpdateZoom();
    }

    private static void OnColorSchemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not ZaremCodeEditor codeEditor)
            return;

        // Local handler to update colors
        void UpdateHandled(object? sender, EventArgs e) => codeEditor.UpdateColorScheme();

        // Unsubscribe from old value
        if (arg.OldValue is AssemblySyntaxColorScheme old)
            old.Updated -= UpdateHandled;

        // Apply new color scheme and subscribe to updates
        codeEditor.UpdateColorScheme();
        codeEditor.ColorScheme?.Updated += UpdateHandled;
    }

    private static void OnLogAnnotationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        if (d is not ZaremCodeEditor codeEditor)
            return;
    }

    private void UpdateText()
    {
        // Get current text, and check if it matches
        // Nothing to be done if the text is unchanged.
        Document.GetText(TextGetOptions.None, out var curr);
        if (Text == curr)
            return;

        // Set the text
        Document.SetText(TextSetOptions.None, Text);

        // TODO: Improve
        _ = UpdateSyntaxHighlightingAsync();
    }

    private async void UpdateColorScheme()
    {
        if (ColorScheme is null)
            return;

        // This is not great
        Background = new SolidColorBrush(ColorScheme.BackgroundColor);

        await UpdateSyntaxHighlightingAsync();
    }
}
