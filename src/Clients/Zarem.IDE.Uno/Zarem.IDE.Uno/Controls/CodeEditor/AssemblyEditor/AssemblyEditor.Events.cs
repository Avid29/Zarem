// Avishai Dernis 2025

namespace Zarem.WinUI.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private void AssemblyEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // While loaded, detach the loaded event and attach unloaded event
        this.Loaded -= AssemblyEditor_Loaded;
        this.Unloaded += AssemblyEditBox_Unloaded;
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
