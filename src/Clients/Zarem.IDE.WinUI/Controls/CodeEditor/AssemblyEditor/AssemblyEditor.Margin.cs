// Avishai Dernis 2026

namespace Zarem.IDE.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private void SetupMargins()
    {
        if (!TryGetEditor(out var editor))
            return;
        
        // TODO: Enable breakpoints

        //// Move line numbers to margin 1
        //editor.SetMarginTypeN(1, MarginType.Number);
        //editor.SetMarginMaskN(1, 0);    // Clear markers from line 1

        //// Setup breakpoints on margin 0
        //editor.SetMarginTypeN(0, MarginType.Symbol);
        //editor.SetMarginWidthN(0, 30);
        //editor.SetMarginSensitiveN(0, true);
        //editor.SetMarginMaskN(0, 1 << 1);
        //editor.MarginLeft = 8;

        //// Define the breakpoint marker 
        //editor.MarkerDefine(1, MarkerSymbol.Circle);
        //editor.MarkerSetBack(1, ToInt(Colors.Red));

        //// Define a ghost breakpoint marker for hovering
        //editor.MarkerDefine(2, MarkerSymbol.Circle);
        //editor.MarkerSetBack(2, ToInt(Colors.LightGray));
    }

    private void SetBreakpoint(long line)
    {
        if (!TryGetEditor(out var editor))
            return;

        // TODO: Actually set breakpoints and sync with breakpoint collection
        // For now, just make a visual indicator
        uint markerMask = (uint)editor.MarkerGet(line);
        if ((markerMask & (1 << 1)) != 0)
        {
            editor.MarkerDelete(line, 1);
        }
        else
        {
            editor.MarkerAdd(line, 1);
        }
    }
}
