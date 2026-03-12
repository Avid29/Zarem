// Avishai Dernis 2026

using System;
using Zarem.Emulator.Events;

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for the CPU state.
/// </summary>
public interface ICpu
{
    /// <summary>
    /// An event invoked when a trap occurs.
    /// </summary>
    event EventHandler<ICpu, TrapEventArgs>? TrapOccurred;

    /// <summary>
    /// An event invoked when a breakpoint is hit.
    /// </summary>
    event EventHandler<TrapEventArgs>? BreakpointHit;

    /// <summary>
    /// Gets the CPU architecture's name.
    /// </summary>
    string ArchitectureName { get; }

    /// <summary>
    /// Gets or sets the current program counter.
    /// </summary>
    ulong ProgramCounter { get; set; }

    ///// <summary>
    ///// Gets the register info for the CPU.
    ///// </summary>
    // TODO: Add debugger interface to expose register info
    //IRegisterGroup Registers { get; }
}
