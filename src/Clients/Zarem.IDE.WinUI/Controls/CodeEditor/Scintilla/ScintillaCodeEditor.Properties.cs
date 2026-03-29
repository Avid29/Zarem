// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using System;

namespace Zarem.IDE.Controls.CodeEditor.Scintilla;

public partial class ScintillaCodeEditor
{
    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="Text"/> property.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(ScintillaCodeEditor), new PropertyMetadata(string.Empty, OnTextPropertyChanged));

    public static readonly DependencyProperty LineProperty =
        DependencyProperty.Register(nameof(Line), typeof(long), typeof(ScintillaCodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register(nameof(Column), typeof(long), typeof(ScintillaCodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(int), typeof(ScintillaCodeEditor), new PropertyMetadata(100, OnZoomPropertyChanged));

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

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not ScintillaCodeEditor codeEditor)
            return;

        codeEditor.UpdateText();
    }

    private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not ScintillaCodeEditor codeEditor)
            return;

        codeEditor.UpdatePosition();
    }

    private static void OnZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not ScintillaCodeEditor codeEditor)
            return;

        codeEditor.UpdateZoom();
    }

    private void UpdateText()
    {
        // Retrieve the editor
        var editor = _childEditor?.Editor;
        if (editor is null)
            return;

        // Get current text, and check if it matches
        // Nothing to be done if the text is unchanged.
        var text = editor.GetText(editor.Length);
        if (Text == text)
            return;
        
        // Set the text and ensure proper line endings
        editor.SetText(Text);
        editor.ConvertEOLs(WinUIEditor.EndOfLine.CrLf);

        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdatePosition()
    {
        // Retrieve the editor
        var editor = _childEditor?.Editor;
        if (editor is null)
            return;

        // Get current position, and check if it matches
        var pos = editor.CurrentPos;
        var line = editor.LineFromPosition(pos);
        var col = editor.GetColumn(pos);

        if (Column != col)
        {
            editor.CurrentPos = editor.FindColumn(Line, Column);
        }
        else if (Line != line)
        {
            editor.GotoLine(Line);
        }
    }

    private void UpdateZoom()
    {
        // Retrieve the editor
        var editor = _childEditor?.Editor;
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
