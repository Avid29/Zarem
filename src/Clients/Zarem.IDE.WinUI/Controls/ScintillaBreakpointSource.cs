// Avishai Dernis 2026

using System.Collections.Generic;
using WinUIEditor;
using Zarem.IDE.Controls.CodeEditor;
using Zarem.IDE.Controls.CodeEditor.Scintilla;
using Zarem.Models.Breakpoints;

namespace Zarem.IDE.Controls;

public class ScintillaBreakpointSource : IBreakpointSource
{
    private readonly Dictionary<BreakpointIdentity, int> _idToHandle = [];
    private readonly Dictionary<int, BreakpointIdentity> _handleToId = [];
    private readonly Editor _editor;

    public ScintillaBreakpointSource(Editor editor, BreakpointCollection breakpoints)
    {
        BreakpointCollection = breakpoints;
        _editor = editor;

        BreakpointCollection.Source = null;

        foreach (var bp in breakpoints.Breakpoints)
            MarkBreakpoint(bp);

        BreakpointCollection.Source = this;
    }

    public BreakpointCollection BreakpointCollection { get; }

    public void SetBreakpoint(long line)
    {
        var bp = BreakpointCollection.Add((ulong)line);
        MarkBreakpoint(bp);
    }

    public void RemoveBreakpoint(long line)
    {
        // Find the breakpoint based on the markers on the line
        BreakpointIdentity? bp = null;
        int handle = -1;
        for (int i = 0; i < _editor.MarkerNumberFromLine(line, 0); i++)
        {
            handle = _editor.MarkerHandleFromLine(line, i);
            if (_handleToId.TryGetValue(handle, out bp))
                break;
        }

        // Breakpoint not found
        if (bp is null)
            return;

        // Remove the breakpoint
        BreakpointCollection.Remove(bp);
        _editor.MarkerDeleteHandle(handle);
    }

    public void ClearBreakpoints()
    {
        foreach (var (bp, handle) in _idToHandle)
        {
            BreakpointCollection.Remove(bp);
            _editor.MarkerDeleteHandle(handle);
        }

        _idToHandle.Clear();
        _handleToId.Clear();
    }

    public ulong? GetBreakpointLine(BreakpointIdentity id)
    {
        if (!_idToHandle.TryGetValue(id, out var handle))
            return null;

        // TODO: Handle when the handle does not exist
        return (ulong)_editor.MarkerLineFromHandle(handle);
    }

    private void MarkBreakpoint(BreakpointIdentity bp)
    {
        //int handle = _editor.MarkerAdd((long)bp.Line, ScintillaCodeEditor.BreakpointMarkerIndex);
        //_idToHandle[bp] = handle;
        //_handleToId[handle] = bp;
    }
}
