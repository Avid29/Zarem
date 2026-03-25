// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Emulator.Machine.Devices.Interfaces;
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
    /// Advance the computer one tick.
    /// </summary>
    /// <remarks>
    /// NOTE:
    /// Machines don't work on one big clock. 
    /// Ideally the processor, devices, and everything should be running on seperate threads.
    /// </remarks>
    void Tick();

    /// <summary>
    /// Loads a module into the computer's memory.
    /// </summary>
    /// <param name="module"></param>
    void Load(Module module);
}
