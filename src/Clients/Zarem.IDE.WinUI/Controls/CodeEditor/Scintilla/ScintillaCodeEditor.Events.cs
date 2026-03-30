// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using WinUIEditor;

namespace Zarem.IDE.Controls.CodeEditor.Scintilla;

public partial class ScintillaCodeEditor
{
    private void CodeEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // While loaded, detach the loaded event and attach unloaded event
        this.Loaded -= CodeEditor_Loaded;
        this.Unloaded += CodeEditor_Unloaded;

        if (!TryGetEditor(out var editor))
            return;

        _childEditor.Focus(FocusState.Keyboard);

        editor.Modified += OnModified;
        editor.ZoomChanged += OnZoomChanged;
        editor.UpdateUI += OnUpdateUI;
        editor.StyleNeeded += OnStyleNeeded;
        editor.MarginClick += OnMarginClicked;
        _childEditor.SyntaxHighlightingApplied += OnSyntaxHighlightingApplied; ;
        _childEditor.HighlightingLanguage = "asm";
    }

    private void OnSyntaxHighlightingApplied(object? sender, ElementTheme e)
    {
        SetUpHighlighting();
    }

    private void OnStyleNeeded(Editor sender, StyleNeededEventArgs args)
    {
        UpdateSyntaxHighlighting();
    }

    private void OnZoomChanged(Editor sender, ZoomChangedEventArgs args)
    {
        var factor = sender.Zoom;
        Zoom = ZoomFactorToPercentage(BaseFontSize, factor);
    }

    private void OnModified(Editor sender, ModifiedEventArgs args)
    {
        Text = sender.GetText(sender.Length);
    }

    private void OnUpdateUI(Editor sender, UpdateUIEventArgs args)
    {
        var pos = sender.CurrentPos;

        Line = sender.LineFromPosition(pos);
        Column = sender.GetColumn(pos);
    }

    private void OnMarginClicked(Editor sender, MarginClickEventArgs args)
    {
        if (_breakpoints is null)
            return;

        var line = sender.LineFromPosition(args.Position);

        switch (args.Margin)
        {
            case 0:
                ToggleBreakpoint(line);
                break;
        }
    }

    private void CodeEditor_Unloaded(object sender, RoutedEventArgs e)
    {
        // Restore the loaded event and detach unloaded event
        this.Loaded += CodeEditor_Loaded;
        this.Unloaded -= CodeEditor_Unloaded;

        //if (!TryGetEditor(out var editor))
        //    return;

        //editor.UpdateUI -= Editor_UpdateUI;
    }
}
