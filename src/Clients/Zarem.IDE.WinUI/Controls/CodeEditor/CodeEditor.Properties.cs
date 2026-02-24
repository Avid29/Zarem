// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using System;

namespace Zarem.WinUI.Controls.CodeEditor;

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

        if (Column != col)
        {
            editor.CurrentPos = editor.FindColumn(line, col);
        }
        else if (Line != line)
        {
            editor.GotoLine(line);
        }
    }
}
