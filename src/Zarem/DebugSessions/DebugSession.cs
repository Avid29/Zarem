// Avishai Dernis 2026

using Zarem.Debugger;
using Zarem.Emulator;
using Zarem.Helpers;
using Zarem.Models;
using Zarem.Models.Breakpoints;

namespace Zarem.DebugSessions;

/// <summary>
/// A class for managing an emulator during a debug session.
/// </summary>
public class DebugSession
{
    private readonly IProject _project;
    private readonly Module _module;
    private readonly LineResolver? _lineResolver;

    internal DebugSession(IProject project, Module module, Zaremulator emulator, Zebugger? debugger = null)
    {
        _project = project;
        _module = module;

        Emulator = emulator;
        Debugger = debugger;

        emulator.Load(module);

        if (debugger is not null)
        {
            if (_module.DebugLines is not null)
            {
                _lineResolver = new LineResolver(_module.DebugLines);
            }

            SetupBreakpoints();
        }
    }

    /// <summary>
    /// Gets the emulator managed by the debug session.
    /// </summary>
    public Zaremulator Emulator { get; }

    /// <summary>
    /// Gets the debugger attached to the emulator.
    /// </summary>
    public Zebugger? Debugger { get; }

    private void SetupBreakpoints()
    {
        if (Debugger is null)
            return;

        foreach (var file in _project.SourceFiles)
        {
            foreach (var bp in file.Breakpoints.Breakpoints)
            {
                BindBreakpoint(bp);
            }
        }
    }

    private void BindBreakpoint(BreakpointIdentity bp)
    {
        var address = _lineResolver?.GetAddress(bp.Parent.File.FullPath, bp.Line);
        if (address?.VirtualAddress is not null)
        {
            Debugger?.SetBreakpoint(address.Value.VirtualAddress.Value);
        }
    }
}
