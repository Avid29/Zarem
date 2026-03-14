// Avishai Dernis 2026

using Microsoft.Extensions.Logging;
using Microsoft.UI;
using System;
using WinUIEditor;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private int _executionLineHandle = -1;

    public const int BreakpointMarkerIndex = 2;
    public const int ExecutionPointIndex = 3;

    private void SetupMargins()
    {
        if (!TryGetEditor(out var editor))
            return;

        // Move line numbers to margin 1
        editor.SetMarginTypeN(1, MarginType.Number);
        editor.SetMarginMaskN(1, 0);    // Clear markers from line 1

        // Setup breakpoints on margin 0
        int margin0Mask = 0;
        editor.SetMarginTypeN(0, MarginType.Symbol);
        editor.SetMarginWidthN(0, 30);
        editor.SetMarginSensitiveN(0, true);
        editor.MarginLeft = 8;

        // Define the breakpoint marker 
        editor.MarkerDefine(BreakpointMarkerIndex, MarkerSymbol.Circle);
        editor.MarkerSetBack(BreakpointMarkerIndex, ToInt(Colors.Red));
        margin0Mask |= (1 << BreakpointMarkerIndex);

        // Define a ghost breakpoint marker for hovering
        //editor.MarkerDefine(2, MarkerSymbol.Circle);
        //editor.MarkerSetBack(2, ToInt(Colors.LightGray));

        // Define execution markers
        editor.MarkerDefine(ExecutionPointIndex, MarkerSymbol.LeftRect);
        editor.MarkerSetBack(ExecutionPointIndex, ToInt(Colors.Yellow));
        editor.MarkerSetFore(ExecutionPointIndex, ToInt(Colors.Black));
        margin0Mask |= (1 << ExecutionPointIndex);

        // Set margin mask
        editor.SetMarginMaskN(0, margin0Mask);
    }

    private void ToggleBreakpoint(long line)
    {
        if (!TryGetEditor(out var editor))
            return;

        // TODO: Actually set breakpoints and sync with breakpoint collection
        // For now, just make a visual indicator
        uint markerMask = (uint)editor.MarkerGet(line);
        if ((markerMask & (1 << BreakpointMarkerIndex)) != 0)
        {
            _breakpoints?.RemoveBreakpoint(line);
        }
        else
        {
            _breakpoints?.SetBreakpoint(line);
        }
    }

    private void UpdateExecutingLine()
    {
        if (!TryGetEditor(out var editor))
            return;

        // Clear existing marker
        editor.MarkerDeleteHandle(_executionLineHandle);
        _executionLineHandle = -1;

        // Clear existing highlight
        editor.IndicatorCurrent = ExecutingLineIndicatorIndex;
        editor.IndicatorClearRange(0, editor.TextLength);

        if (ExecutingLine.HasValue)
        {
            // Get line range info
            var line = (long)ExecutingLine.Value;

            // Set new marker
            _executionLineHandle = editor.MarkerAdd(line, ExecutionPointIndex);

            // Find the position to highlight the executing line
            var lineStart = editor.PositionFromLine(line);
            var length = editor.LineLength(line);

            // Adjust line position to only highlight tokens
            if (_tokenizedAssembly is not null)
            {
                var asmLine = _tokenizedAssembly[(int)line];
                if (asmLine.Count > 0)
                {
                    var realStart = asmLine[0].Location.Index;
                    realStart = _locationMapper[realStart].Index;
                    length -= realStart - lineStart;
                    lineStart = realStart;
                }
            }

            // Apply the new highlight
            editor.IndicatorCurrent = ExecutingLineIndicatorIndex;
            editor.IndicatorFillRange(lineStart, length);

            // Adjust view to ensure the line is comfortably on screen
            editor.EnsureVisible(line);
            long firstVisible = editor.FirstVisibleLine;
            long lastVisible = firstVisible + editor.LinesOnScreen;
            if (line <= firstVisible + 2 || line > lastVisible - 2)
            {
                long targetFirstLine = line - (editor.LinesOnScreen / 2);
                editor.FirstVisibleLine = Math.Max(0, targetFirstLine);
            }
        }
    }
}
