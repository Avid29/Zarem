// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WinUIEditor;
using Zarem.Models.Breakpoints;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor.Scintilla;

[TemplatePart(Name = CodeEditorPartName, Type = typeof(CodeEditorControl))]
public sealed partial class ScintillaCodeEditor : Control, ICodeEditor
{
    private const string CodeEditorPartName = "PART_CodeEditorControl";
    private const int BaseFontSize = 11;

    private CodeEditorControl? _childEditor;
    private ScintillaBreakpointSource? _breakpoints;

    private bool _isUpdatingText;

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

        SetupKeybinds();

        SetUpHighlighting();
        SetupIndicators();
        SetupMargins();

        // Apply the current text
        UpdateText();
    }

    public void RegisterBreakpointSource(BreakpointCollection breakpoints)
    {
        if (!TryGetEditor(out var editor))
            return;

        editor.MarkerDeleteAll(BreakpointMarkerIndex);
        _breakpoints = new ScintillaBreakpointSource(editor, breakpoints);
    }

    public void UnregisterBreakpointSource()
    {
        _breakpoints?.BreakpointCollection.Source = null;
        _breakpoints = null;
    }

    public void ResetHistory()
    {
        if (!TryGetEditor(out var editor))
            return;

        editor.EmptyUndoBuffer();
    }

    [MemberNotNullWhen(true, nameof(_childEditor))]
    private bool TryGetEditor([NotNullWhen(true)] out Editor? editor)
    {
        editor = _childEditor?.Editor;
        return editor is not null;
    }

    private long GetMappedIndex(SourceLocation location) => GetMappedIndex(location.Line, location.Column, 1, out _);

    private long GetMappedIndex(SourceRange location, out long end)
        => GetMappedIndex(location.Start.Line, location.Start.Column, (int)location.Size, out end);

    private long GetMappedIndex(long line, int column) => GetMappedIndex(line, column, 1, out _);

    private long GetMappedIndex(long line, int column, int sizeIn, out long sizeOut)
    {
        sizeOut = -1;
        if (!TryGetEditor(out var editor))
            return -1;

        // Get the utf8 index of the start of the line
        var lineStartUtf8 = editor.PositionFromLine(line);
        string lineText = editor.GetLine(line);

        // Get the safe margins to avoid out of range errors, and calculate the utf8 offset of the column
        int safeColumn = Math.Min(column, lineText.Length);
        int safeSize = Math.Min(sizeIn, lineText.Length - safeColumn);

        // Get the utf8 offset of the column, and calculate the utf8 length of the token
        int columnOffsetUtf8 = Encoding.UTF8.GetByteCount(lineText[..safeColumn]);
        long startUtf8 = lineStartUtf8 + columnOffsetUtf8;
        sizeOut = Encoding.UTF8.GetByteCount(lineText.AsSpan(safeColumn, safeSize));

        return startUtf8;
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
