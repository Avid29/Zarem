// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using WinUIEditor;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class CodeEditor
{
    private void CodeEditor_Loaded(object sender, RoutedEventArgs e)
    {
        // While loaded, detach the loaded event and attach unloaded event
        this.Loaded -= CodeEditor_Loaded;
        this.Unloaded += CodeEditor_Unloaded;

        if (!TryGetEditor(out var editor))
            return;

        editor.Modified += Editor_Modified;
        editor.ZoomChanged += Editor_ZoomChanged;
        editor.UpdateUI += Editor_UpdateUI;
    }

    private void Editor_ZoomChanged(Editor sender, ZoomChangedEventArgs args)
    {
        var factor = sender.Zoom;
        Zoom = ZoomFactorToPercentage(BaseFontSize, factor);
    }

    private void Editor_Modified(Editor sender, ModifiedEventArgs args)
    {
        Text = sender.GetText(sender.Length);
    }

    private void Editor_UpdateUI(Editor sender, UpdateUIEventArgs args)
    {
        var pos = sender.CurrentPos;

        Line = sender.LineFromPosition(pos) + 1;
        Column = sender.GetColumn(pos) + 1;
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
