// Avishai Dernis 2026

using System;
using Zarem.Debugger;
using Zarem.Emulator;
using Zarem.Helpers;
using Zarem.Models;
using Zarem.Models.Breakpoints;

namespace Zarem.DebugSessions;

/// <summary>
/// A class for managing an emulator during a debug session.
/// </summary>
public class DebugSession : IDisposable
{
    private readonly IProject _project;
    private readonly Module _module;

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
                LineResolver = new LineResolver(_module.DebugLines);
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
    
    /// <summary>
    /// Gets the line resolver for addresses in the debug session.
    /// </summary>
    public LineResolver? LineResolver { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var file in _project.SourceFiles)
        {
            file.Breakpoints.BreakpointAdded -= Breakpoints_BreakpointAdded;
            file.Breakpoints.BreakpointRemoved -= Breakpoints_BreakpointRemoved;
        }
    }

    private void SetupBreakpoints()
    {
        if (Debugger is null)
            return;

        foreach (var file in _project.SourceFiles)
        {
            foreach (var bp in file.Breakpoints.Breakpoints)
            {
                ToggleBreakpoint(bp);
            }

            file.Breakpoints.BreakpointAdded += Breakpoints_BreakpointAdded;
            file.Breakpoints.BreakpointRemoved += Breakpoints_BreakpointRemoved;
        }
    }

    private void Breakpoints_BreakpointAdded(object? sender, BreakpointIdentity e)
    {
        ToggleBreakpoint(e);
    }

    private void Breakpoints_BreakpointRemoved(object? sender, BreakpointIdentity e)
    {
        ToggleBreakpoint(e, false);
    }

    private void ToggleBreakpoint(BreakpointIdentity bp, bool enable = true)
    {
        var address = LineResolver?.GetAddress(bp.Parent.File.FullPath, bp.Line);
        if (address?.VirtualAddress is not null)
        {
            var vAddress = address.Value.VirtualAddress.Value;
            if (enable)
            {
                Debugger?.SetBreakpoint(vAddress);
            }
            else
            {
                Debugger?.RemoveBreakpoint(vAddress);
            }
        }
    }
}
