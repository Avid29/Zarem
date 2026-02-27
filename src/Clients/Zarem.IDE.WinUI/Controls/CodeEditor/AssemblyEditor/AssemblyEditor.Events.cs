// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using WinUIEditor;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private void AssemblyEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // While loaded, detach the loaded event and attach unloaded event
        this.Loaded -= AssemblyEditor_Loaded;
        this.Unloaded += AssemblyEditBox_Unloaded;

        if (!TryGetEditor(out var editor))
            return;

        editor.StyleNeeded += Editor_StyleNeeded;
        editor.MarginClick += Editor_MarginClick;
        ChildEditor.SyntaxHighlightingApplied += CodeEditor_SyntaxHighlightingApplied;
        ChildEditor.HighlightingLanguage = "asm";
    }

    private void Editor_MarginClick(Editor sender, MarginClickEventArgs args)
    {
        var line = sender.LineFromPosition(args.Position);

        switch (args.Margin)
        {
            case 0:
                SetBreakpoint(line);
                break;
        }
    }

    private async void Editor_StyleNeeded(Editor sender, StyleNeededEventArgs args)
    {
        SetupMargins();

        await RunAssemblerAsync();
        UpdateSyntaxHighlighting();
    }

    private void CodeEditor_SyntaxHighlightingApplied(object? sender, ElementTheme e)
    {
        SetupHighlighting();
    }

    private void AssemblyEditBox_Unloaded(object sender, RoutedEventArgs e)
    {
        // Restore the loaded event and detach unloaded event
        this.Loaded += AssemblyEditor_Loaded;
        this.Unloaded -= AssemblyEditBox_Unloaded;
    }
}
