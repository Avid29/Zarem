// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Threading;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Models;

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for an emulated computer.
/// </summary>
public interface IComputer : IDisposable
{
    /// <summary>
    /// An event invoked when a shutdown is requested.
    /// </summary>
    event EventHandler? ShutdownRequested;

    /// <summary>
    /// Gets an interface to the CPU state info.
    /// </summary>
    ICpu Cpu { get; }

    /// <summary>
    /// Gets an interface for the computer's memory system.
    /// </summary>
    IMemorySystem Memory { get; }

    /// <summary>
    /// Gets an <see cref="IEnumerable{IDevice}"/> of all the devices in the <see cref="IComputer"/>.
    /// </summary>
    IEnumerable<IDevice> Devices { get; }

    /// <summary>
    /// Starts or resumes execution. 
    /// Execution continues until the cancellation token is signaled 
    /// or an internal shutdown/trap occurs.
    /// </summary>
    void Run(CancellationToken ct);

    /// <summary>
    /// Loads a module into the computer's memory.
    /// </summary>
    /// <param name="module"></param>
    void Load(Module module);
}
