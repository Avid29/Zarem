// Avishai Dernis 2025

using System;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.CoProcessors;
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
    public MipsCpu(MIPSEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;

        Tlb = new MipsTlb();
        Memory = new MemorySystem(bus, Tlb);
    }

    /// <summary>
    /// Gets the cpu's general purpose register file.
    /// </summary>
    public abstract IRegisterFile RegisterFile { get; }

    /// <summary>
    /// Gets the floating-point coprocessor.
    /// </summary>
    public abstract IFloatProcessor FloatProcessor { get; }

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    public MipsTlb Tlb { get; }

    /// <summary>
    /// Gets the emulation configuration.
    /// </summary>
    public MIPSEmulatorConfig Config { get; }

    /// <summary>
    /// Gets the system memory
    /// </summary>
    public IMemorySystem Memory { get; }

    /// <inheritdoc/>
    public string ArchitectureName => "MIPS";

    /// <inheritdoc cref="ICpu.ProgramCounter"/>
    public abstract ulong ProgramCounter { get; set; }

    /// <summary>
    /// Gets the jump address in the delay slot.
    /// </summary>
    public abstract ulong? DelaySlot { get; }

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public abstract ulong this[int reg] { get; set; }

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
