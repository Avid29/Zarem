// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Zarem.Debugger.Handlers;
using Zarem.Debugger.Models;
using Zarem.Debugger.Models.Enums;
using Zarem.Debugger.Viewer;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models;

namespace Zarem.Debugger;

/// <summary>
/// A debugger than can be attached to the emulator.
/// </summary>
public class Zebugger
{
    private readonly IDebugHandler _handler;
    private readonly IComputer _computer;
    private readonly Dictionary<ulong, Breakpoint> _breakpoints = [];

    private Breakpoint? _pointToRestore;    // This tracks the user BP to restore after executing
    private Breakpoint? _internalTrap;      // A temporary breakpoint for restorations
    private Breakpoint? _stepTrap;          // A temporary breakpoint for stepping
    private BreakpointHitEventArgs? _trapEvent;

    /// <summary>
    /// An invoked when the debugger halted the execution.
    /// </summary>
    public event EventHandler<Zebugger, ulong>? Halted;

    /// <summary>
    /// An invoked when the debugger resumes execution.
    /// </summary>
    public event EventHandler? Resumed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Zebugger"/> class.
    /// </summary>
    public Zebugger(IDebugHandler handler, IComputer computer)
    {
        _handler = handler;
        _computer = computer;
        Viewer = _handler.GetDebugViewer(computer);

        _computer.Cpu.BreakpointHit += OnBreakpointHit;
    }

    /// <summary>
    /// Gets the <see cref="IDebugViewer"/> for the debug session.
    /// </summary>
    public IDebugViewer? Viewer { get; }

    /// <summary>
    /// Gets the currently halting breakpoint.
    /// </summary>
    public Breakpoint? CurrentBreakpointPoint { get; private set; }

    /// <summary>
    /// Steps using the specified step mode.
    /// </summary>
    /// <param name="mode">The style of step to perform.</param>
    public void Step(StepMode mode)
    {
        Action stepAction = mode switch
        {
            StepMode.Step => Step,
            StepMode.StepOver => StepOver,
            StepMode.StepOut => StepOut,
            StepMode.Continue or _ => Continue,
        };

        stepAction();
    }

    /// <summary>
    /// Resumes execution.
    /// </summary>
    public void Continue() => ResumeExecution();

    /// <summary>
    /// Steps a single instruction.
    /// </summary>
    public void Step() => SetStepAndResume(_handler.GetStepAddress(_computer));

    /// <summary>
    /// Steps over a call or "and link" instruction to when it return. Or just steps one instruction.
    /// </summary>
    public void StepOver() => SetStepAndResume(_handler.GetStepOverAddress(_computer));

    /// <summary>
    /// Steps out to the current return address.
    /// </summary>
    public void StepOut() => SetStepAndResume(_handler.GetStepOutAddress(_computer));

    /// <summary>
    /// Sets a breakpoint in memory.
    /// </summary>
    /// <param name="address">The address to set the breakpoint</param>
    public void SetBreakpoint(ulong address)
    {
        // Initialize the breakpoint
        if (!_breakpoints.TryGetValue(address, out var bp))
        {
            bp = new Breakpoint(address, _handler.BreakpointBytes.Length);
            _breakpoints.Add(address, bp);
        }

        // Enable the breakpoint
        ToggleBreakpoint(bp, true);
    }

    /// <summary>
    /// Removes a breakpoint from memory.
    /// </summary>
    /// <param name="address"></param>
    public void RemoveBreakpoint(ulong address)
    {
        // Retreive the breakpoint
        if (!_breakpoints.TryGetValue(address, out var bp))
            return;

        // Disable the breakpoint
        ToggleBreakpoint(bp, false);
        _breakpoints.Remove(address);

        if (_pointToRestore == bp)
            _pointToRestore = null;
    }

    private void OnBreakpointHit(object? sender, BreakpointHitEventArgs e)
    {
        _trapEvent = e;
        var address = _computer.Cpu.ProgramCounter;

        // Always restore a user breakpoint if we were stepping off one
        if (_pointToRestore is not null)
        {
            ToggleBreakpoint(_pointToRestore, true);
            _pointToRestore = null;
        }

        // Check and clear step and internal traps
        bool hitStep = CheckAndClearTempBreakpoint(ref _stepTrap, out var stepPoint);
        bool hitInternal = CheckAndClearTempBreakpoint(ref _internalTrap, out _);

        // If we hit or there's a user breakpoint here, we hit a breakpoint
        if (_breakpoints.TryGetValue(address, out var userBp) || hitStep)
        {
            // This is a safe null suppression. Step point cannot be null since hitStep is true
            CurrentBreakpointPoint = userBp ?? stepPoint!;
            _pointToRestore = userBp;
            ToggleBreakpoint(CurrentBreakpointPoint, false);
        }
        else
        {
            // Otherwise we hit an internal breakpoint
            ResumeExecution();
            return;
        }

        Halted?.Invoke(this, address);
    }

    private void SetStepAndResume(ulong targetAddress)
    {
        if (_trapEvent is null)
            return;

        if (!_breakpoints.TryGetValue(targetAddress, out var bp))
        {
            bp = new Breakpoint(targetAddress, _handler.BreakpointBytes.Length);
        }

        _stepTrap = bp;
        ToggleBreakpoint(_stepTrap, true);

        ResumeExecution();
    }

    private void ResumeExecution()
    {
        if (_trapEvent is null)
            return;

        // Setup an internal trap as a restoration point, if needed
        if (_pointToRestore is not null)
        {
            // Only set a trap if the there's not already a breakpoint there, user or step
            var nextAddr = _handler.GetStepAddress(_computer);
            if (!_breakpoints.ContainsKey(nextAddr) && !(_stepTrap?.Address == nextAddr))
            {
                _internalTrap = new Breakpoint(nextAddr, _handler.BreakpointBytes.Length);
                ToggleBreakpoint(_internalTrap, true);
            }
        }

        Resumed?.Invoke(this, EventArgs.Empty);
        _trapEvent.Resume();
        _trapEvent = null;
        CurrentBreakpointPoint = null;
    }

    private void ToggleBreakpoint(Breakpoint bp, bool enabled)
    {
        if (bp.IsApplied == enabled)
            return;

        if (enabled)
        {
            // Enable the breakpoint
            _computer.Memory.Virtual.Read(bp.Address, bp.Swap);
            _computer.Memory.Virtual.Write(bp.Address, _handler.BreakpointBytes);
        }
        else
        {
            // Disable the breakpoint
            _computer.Memory.Virtual.Write(bp.Address, bp.Swap);
        }

        bp.IsApplied = enabled;
    }

    private bool CheckAndClearTempBreakpoint(ref Breakpoint? temp, [NotNullWhen(true)] out Breakpoint? bp)
    {
        // Ensure the breakpoint is disabled
        if (temp is not null)
        {
            ToggleBreakpoint(temp, false);
        }

        // Check if the breakpoint was hit
        var address = _computer.Cpu.ProgramCounter;
        bool hit = temp is not null && address == temp.Address;

        // Clear the breakpoint
        bp = temp;
        temp = null;
        return hit;
    }
}
