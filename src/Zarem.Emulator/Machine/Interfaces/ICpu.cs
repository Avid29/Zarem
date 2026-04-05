// Avishai Dernis 2026

using System;
using Zarem.Emulator.Events;

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for the CPU state.
/// </summary>
public interface ICpu : IDisposable
{
    /// <summary>
    /// An event invoked when a breakpoint is hit.
    /// </summary>
    event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <summary>
    /// Gets the CPU architecture's name.
    /// </summary>
    string ArchitectureName { get; }

    /// <summary>
    /// Gets or sets the current program counter.
    /// </summary>
    ulong ProgramCounter { get; set; }

    /// <summary>
    /// Gets the general-purpose register file of the processor.
    /// </summary>
    IRegisterFile RegisterFile { get; }

    /// <summary>
    /// Gets the system memory.
    /// </summary>
    MemorySystem Memory { get; }
}
