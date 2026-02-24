// Avishai Dernis 2025

using Microsoft.UI.Xaml;

namespace Zarem.WinUI.Controls.CodeEditor;

public partial class CodeEditor
{
    private void CodeEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // While loaded, detach the loaded event and attach unloaded event
        this.Loaded -= CodeEditor_Loaded;
        this.Unloaded += CodeEditor_Unloaded;
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
