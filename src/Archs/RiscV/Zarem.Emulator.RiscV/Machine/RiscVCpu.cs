// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models;
using Zarem.Emulator.TrapHandlers;
using Zarem.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public abstract class RiscVCpu<T> : IRiscVCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <inheritdoc/>
    public event EventHandler? ShutdownRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVCpu(RiscVEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;
        RegisterFile = new();
        Tlb = new RiscVTlb();
        Memory = new MemorySystem(bus, Tlb);
    }

    /// <inheritdoc/>
    public string ArchitectureName => "RISC-V";

    /// <inheritdoc/>
    public Endianness Endianness => Endianness.Little;

    /// <inheritdoc/>
    public RiscVEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public T ProgramCounter { get; set; }

    /// <inheritdoc/>
    ulong ICpu.ProgramCounter
    {
        get => ulong.CreateTruncating(ProgramCounter);
        set => ProgramCounter = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public RiscVGPRegisterFile<T> RegisterFile { get; }

    /// <inheritdoc/>
    IRegisterFile ICpu.RegisterFile => RegisterFile;

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    public RiscVTlb Tlb { get; }

    /// <inheritdoc/>
    public MemorySystem Memory { get; }

    /// <inheritdoc/>
    public double MeasuredSpeed { get; set; }

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public T this[RiscVGpRegister reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <inheritdoc/>
    ulong IRiscVCpu.this[RiscVGpRegister reg]
    {
        get => ulong.CreateTruncating(RegisterFile[(int)reg]);
        set => RegisterFile[(int)reg] = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public abstract void Insert(RiscVInstruction instruction, out RiscVTrap trap);

    /// <inheritdoc/>
    public abstract void Run(CancellationToken ct);

    /// <inheritdoc/>
    public void RequestShutdown() => ShutdownRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Handles a trap.
    /// </summary>
    protected void HandleTrap(RiscVTrap trap)
    {
        if (trap is RiscVTrap.None)
            return;

        // Breakpoints are handled by the debugger upon the trap occurring event
        // The host also handles every kind of trap if that's what the config specifies
        if (trap is RiscVTrap.Breakpoint && BreakpointHit is not null)
        {
            // Only wait if a debugger is attached
            var eventArgs = new BreakpointHitEventArgs();
            BreakpointHit.Invoke(this, eventArgs);
            eventArgs.Wait();
        }
        else
        {
            // The host handled the trap, do not emulate it
            // Breakpoints are always handled by the host
            Config.TrapHost?.HandleTrap(new RiscVTrapContext(this, (ulong)trap));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        RegisterFile.Dispose();
    }
}
