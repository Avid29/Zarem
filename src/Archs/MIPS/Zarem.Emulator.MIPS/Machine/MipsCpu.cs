// Avishai Dernis 2025

using System;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public abstract partial class MipsCpu : ICpu<MipsCpu, MipsInstruction, MipsTrap>
{
    /// <inheritdoc/>
    public abstract event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    internal event EventHandler? ShutdownRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu"/> class.
    /// </summary>
    public MipsCpu(MIPSEmulatorConfig config, IMemoryAccessor memory)
    {
        Config = config;

        Tlb = new MipsTlb();
        Memory = memory;

    }

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    public MipsTlb Tlb { get; }

    /// <summary>
    /// Gets the emulation config.
    /// </summary>
    public MIPSEmulatorConfig Config { get; }

    /// <summary>
    /// Gets the system memory
    /// </summary>
    public IMemoryAccessor Memory { get; internal set; }

    /// <inheritdoc/>
    public string ArchitectureName => "MIPS";

    /// <inheritdoc cref="ICpu.ProgramCounter"/>
    public abstract ulong ProgramCounter { get; set; }

    /// <summary>
    /// Requests a shutdown.
    /// </summary>
    public void RequestShutdown() => ShutdownRequested?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    public abstract void Step();

    /// <inheritdoc/>
    public abstract void Insert(MipsInstruction instruction, out MipsTrap trap);

    /// <inheritdoc/>
    public abstract void Dispose();
}
