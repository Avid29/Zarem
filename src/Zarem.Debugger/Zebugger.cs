// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Debugger.Handlers;
using Zarem.Debugger.Models;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Debugger;

/// <summary>
/// A debugger than can be attached to the emulator.
/// </summary>
public class Zebugger
{
    private readonly IDebugHandler _handler;
    private readonly IComputer _computer;
    private readonly Dictionary<ulong, Breakpoint> _breakpoints = [];
    private Breakpoint? _restorePoint;
    private Breakpoint? _tempPoint;
    private TrapEventArgs? _trapEvent;

    /// <summary>
    /// An invoked when the debugger halted the execution.
    /// </summary>
    public event EventHandler<Zebugger, ulong>? Halted;

    /// <summary>
    /// Initializes a new instance of the <see cref="Zebugger"/> class.
    /// </summary>
    public Zebugger(IDebugHandler handler, IComputer computer)
    {
        _handler = handler;
        _computer = computer;

        _computer.Cpu.BreakpointHit += OnBreakpointHit;
    }

    /// <summary>
    /// Gets the currently hit breakpoint
    /// </summary>
    public Breakpoint? GetCurrentBreakpoint()
    {
        var address = _computer.Cpu.ProgramCounter;
        if (_breakpoints.TryGetValue(address, out var breakpoint))
            return breakpoint;

        return null;
    }

    private void OnBreakpointHit(object? sender, TrapEventArgs e)
    {
        _trapEvent = e;

        // Restore the breakpoint if a restoration is queued
        if (_restorePoint is not null)
        {
            ToggleBreakpoint(_restorePoint, true);
            _restorePoint = null;
        }

        // Temporarily disable the breakpoint so the CPU can execute the real instruction when resumed
        // Mark the breakpoint for restoration, though
        // This is also convenient because the instruction's memory is correct
        var current = GetCurrentBreakpoint();
        if (current is not null)
        {
            ToggleBreakpoint(current, false);
            _restorePoint = current;

            // If there's not already a breakpoint there, setup a temporary breakpoint at the next instruction
            // TODO: Variable instruction sizes
            //var nextAddress = current.Address + _handler.InstructionSize;
            var nextAddress = _handler.GetStepAddress(_computer);
            if (!_breakpoints.ContainsKey(nextAddress))
            {
                _tempPoint = new Breakpoint(nextAddress, _handler.BreakpointBytes.Length);
                ToggleBreakpoint(_tempPoint, true);
            }

            // Rewind the program counter
            _computer.Cpu.ProgramCounter -= (ulong)_handler.BreakpointBytes.Length;
        }

        // Temp point is used to enqueue a restoration.
        // Just remove the temp-point and continue
        if (_tempPoint is not null)
        {
            ToggleBreakpoint(_tempPoint, false);
            _tempPoint = null;

            _trapEvent.Resume();
            _trapEvent = null;
            return;
        }

        Halted?.Invoke(this, (ulong)((long)_computer.Cpu.ProgramCounter));
    }

    /// <summary>
    /// Resumes execution.
    /// </summary>
    public void Continue()
    {
        if (_trapEvent is null)
            return;

        // That's it. The restoration point has already been established
        _trapEvent.Resume();
        _trapEvent = null;
    }

    /// <summary>
    /// Steps a single instruction.
    /// </summary>
    public void Step()
    {
        if (_trapEvent is null)
            return;

        // TODO:
    }

    /// <summary>
    /// Steps over a call or "and link" instruction to when it return. Or just steps one instruction.
    /// </summary>
    public void StepOver()
    {
        if (_trapEvent is null)
            return;

        // TODO:
    }

    /// <summary>
    /// Steps out to the current return address.
    /// </summary>
    public void StepOut()
    {
        if (_trapEvent is null)
            return;

        // TODO:
    }

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
}
