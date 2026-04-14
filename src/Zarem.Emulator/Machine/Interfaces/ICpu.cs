// Avishai Dernis 2026

using System;
using System.Threading;
using Zarem.Emulator.Events;
using Zarem.Models.Enums;

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
    /// An event invoked when the processor requests a shutdown of the emulator.
    /// </summary>
    event EventHandler? ShutdownRequested;

    /// <summary>
    /// Gets the CPU architecture's name.
    /// </summary>
    string ArchitectureName { get; }

    /// <summary>
    /// Gets the CPU's endianness.
    /// </summary>
    Endianness Endianness { get; }

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

    /// <summary>
    /// Starts or resumes execution. 
    /// Execution continues until the cancellation token is signaled 
    /// or an internal shutdown/trap occurs.
    /// </summary>
    void Run(CancellationToken ct);

    /// <summary>
    /// Requests a shutdown.
    /// </summary>
    void RequestShutdown();
}
