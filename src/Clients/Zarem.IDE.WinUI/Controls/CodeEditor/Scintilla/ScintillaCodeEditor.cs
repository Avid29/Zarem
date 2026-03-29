// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WinUIEditor;
using Zarem.Helpers;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor.Scintilla;

[TemplatePart(Name = CodeEditorPartName, Type = typeof(CodeEditorControl))]
public sealed partial class ScintillaCodeEditor : Control, ICodeEditor
{
    private const string CodeEditorPartName = "PART_CodeEditorControl";
    private const int BaseFontSize = 11;

    private CodeEditorControl? _childEditor;

    /// <summary>
    /// An event invoked when the <see cref="Text"/> property changes
    /// </summary>
    public event EventHandler? TextChanged;

    public ScintillaCodeEditor()
    {
        DefaultStyleKey = typeof(ScintillaCodeEditor);
    }

    /// <summary>
    /// Navigates to a <see cref="SourceLocation"/>.
    /// </summary>
    /// <param name="location">The position to navigate to.</param>
    public void NavigateToToken(SourceLocation location)
    {
        // Get the editor
        var editor = _childEditor?.Editor;
        if (editor is null)
            return;

        // Get mapped location
        var utf8Position = GetMappedIndex(location);

        // Go to position, and focus the keyboard
        editor.EnsureVisible(location.Line);
        editor.GotoPos(utf8Position);
        _childEditor?.Focus(FocusState.Keyboard);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Setup template parts
        _childEditor = (CodeEditorControl)GetTemplateChild(CodeEditorPartName);

        // Setup events
        this.Loaded += CodeEditor_Loaded;

        // Setup keybinds
        SetupKeybinds();

        // Apply the current text
        UpdateText();
    }

    [MemberNotNullWhen(true, nameof(_childEditor))]
    private bool TryGetEditor([NotNullWhen(true)] out Editor? editor)
    {
        editor = _childEditor?.Editor;
        return editor is not null;
    }

    public void ResetHistory()
    {
        if (!TryGetEditor(out var editor))
            return;

        editor.EmptyUndoBuffer();
    }

    private long GetMappedIndex(SourceLocation location)
    {
        if (!TryGetEditor(out var editor))
            return -1;

        // Get the utf8 index of the start of the line
        var lineStartUtf8 = editor.PositionFromLine(location.Line);

        // Get the text of the line and calculate the utf8 length of the text before the column
        string lineText = editor.GetLine(location.Line);
        int safeColumn = Math.Min(location.Column, lineText.Length);
        int columnOffsetUtf8 = Encoding.UTF8.GetByteCount(lineText[..safeColumn]);

        return lineStartUtf8 + columnOffsetUtf8;
    }

    private static int ZoomPercentageToFactor(int baseSize, int percentage)
    {
        double size = (percentage * baseSize) / 100d;
        int factor = (int)Math.Round(size - baseSize);
        return Math.Clamp(factor, -10, 20);
    }

    private static int ZoomFactorToPercentage(int baseSize, int factor)
    {
        double percentage = ((double)(baseSize + factor) / baseSize) * 100;
        return (int)Math.Round(percentage);
    }
}
