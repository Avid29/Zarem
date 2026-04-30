// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Devices.Interfaces;
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
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <inheritdoc/>
    public event EventHandler? ShutdownRequested;

    /// <summary>
    /// Gets the emulation configuration to follow for computing.
    /// </summary>
    public abstract EmulatorConfig Config { get; }

    /// <inheritdoc/>
    public abstract ICpu Cpu { get; }

    /// <inheritdoc/>
    public abstract MemorySystem Memory { get; }

    /// <inheritdoc/>
    IMemorySystem IComputer.Memory => Memory;

    /// <inheritdoc/>
    public abstract IEnumerable<IDevice> Devices { get; }

    /// <summary>
    /// Requests a shutdown.
    /// </summary>
    public void RequestShutdown() => ShutdownRequested?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    public void Load(Module module)
    {
        using (Stream busStream = this.Memory.Virtual.AsStream())
        {
            foreach (var section in module.Sections.Values)
            {
                // Seek to the Physical Address defined by the Linker
                busStream.Position = (long)section.VirtualAddress;

                // Reset the section stream and copy it into the hardware
                section.Stream.Position = 0;
                section.Stream.CopyTo(busStream);
            }
        }

        // Set the Entry Point
        if (module.EntryAddress.HasValue)
        {
            Cpu.ProgramCounter = module.EntryAddress.Value;
        }
    }

    /// <inheritdoc/>
    public virtual void Run(CancellationToken ct) => Cpu.Run(ct);

    /// <summary>
    /// Maps the devices in the memory bus.
    /// </summary>
    protected abstract void MapDevices(MemoryMapper mapper);

    /// <inheritdoc/>
    protected virtual void OnTrap(BreakpointHitEventArgs e) => BreakpointHit?.Invoke(this, e);

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var device in Devices)
            device.Dispose();

        Cpu.Dispose();
    }
}
