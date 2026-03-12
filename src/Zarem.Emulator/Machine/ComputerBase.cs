// Avishai Dernis 2026

using System;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A base class for <see cref="IComputer"/> implementations.
/// </summary>
public abstract class ComputerBase : IComputer
{
    /// <inheritdoc/>
    public event EventHandler<TrapEventArgs>? TrapOccurred;

    /// <inheritdoc/>
    public abstract ICpu Cpu { get; }

    /// <inheritdoc/>
    public abstract IMemorySystem Memory { get; }

    /// <inheritdoc/>
    public abstract void Load(Module module);

    /// <inheritdoc/>
    public abstract void Tick();

    /// <summary>
    /// Maps the devices in the memory bus.
    /// </summary>
    protected abstract void MapDevices(MemoryMapper mapper);

    /// <inheritdoc/>
    protected virtual void OnTrap(TrapEventArgs e)
    {
        TrapOccurred?.Invoke(this, e);
    }
}
