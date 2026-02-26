// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using System;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class CodeEditor
{
    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="Text"/> property.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CodeEditor), new PropertyMetadata(string.Empty, OnTextChanged));

    public static readonly DependencyProperty LineProperty =
        DependencyProperty.Register(nameof(Line), typeof(long), typeof(CodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register(nameof(Column), typeof(long), typeof(CodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(int), typeof(CodeEditor), new PropertyMetadata(100, OnZoomPropertyChanged));

    /// <summary>
    /// Gets or sets the text contained in the editbox.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the current line.
    /// </summary>
    public long Line
    {
        get => (long)GetValue(LineProperty);
        set => SetValue(LineProperty, value);
    }

    /// <summary>
    /// Gets or sets the current column.
    /// </summary>
    public long Column
    {
        get => (long)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    /// <summary>
    /// Gets or sets the current zoom percentage.
    /// </summary>
    public int Zoom
    {
        get => (int)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not CodeEditor codeEditor)
            return;

        codeEditor.UpdateText();
    }

    private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not CodeEditor codeEditor)
            return;

        codeEditor.UpdatePosition();
    }

    private static void OnZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not CodeEditor codeEditor)
            return;

        codeEditor.UpdateZoom();
    }

    private void UpdateText()
    {
        // Retrieve the editor
        var editor = ChildEditor?.Editor;
        if (editor is null)
            return;

        // Get current text, and check if it matches
        var text = editor.GetText(editor.Length);
        if (Text == text)
            return;

        // The text was not already update to date. Update it
        editor.SetText(Text);
        editor.ConvertEOLs(WinUIEditor.EndOfLine.CrLf);
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdatePosition()
    {
        // Retrieve the editor
        var editor = ChildEditor?.Editor;
        if (editor is null)
            return;

        // Get current position, and check if it matches
        var pos = editor.CurrentPos;
        var line = editor.LineFromPosition(pos);
        var col = editor.GetColumn(pos);

        if (Column - 1 != col)
        {
            editor.CurrentPos = editor.FindColumn(Line - 1, Column - 1);
        }
        else if (Line - 1 != line)
        {
            editor.GotoLine(Line - 1);
        }
    }

    private void UpdateZoom()
    {
        // Retrieve the editor
        var editor = ChildEditor?.Editor;
        if (editor is null)
            return;

        // Get current zoom, and check if it matches
        var factor = editor.Zoom;
        var percentage = ZoomFactorToPercentage(BaseFontSize, factor);

        if (Zoom != percentage)
        {
            editor?.Zoom = ZoomPercentageToFactor(BaseFontSize, Zoom);
        }
    }
}
