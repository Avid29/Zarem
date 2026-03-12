// Avishai Dernis 2026

using System;
using Zarem.Emulator.Events;
using Zarem.Models;

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for an emulated computer.
/// </summary>
public interface IComputer
{
    /// <summary>
    /// An event invoked when a trap occurs.
    /// </summary>
    event EventHandler<TrapEventArgs>? TrapOccurred;

    /// <summary>
    /// Gets an interface to the CPU state info.
    /// </summary>
    ICpu Cpu { get; }

    /// <summary>
    /// Gets an interface for the computer's memory system.
    /// </summary>
    IMemorySystem Memory { get; }

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
