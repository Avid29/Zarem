// Avishai Dernis 2025

using System;
using System.Numerics;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.TrapHandlers;
using Zarem.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A base class representing a processor unit.
/// </summary>
public abstract partial class MipsCpu<T> : IMipsCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <inheritdoc/>
    public event EventHandler? ShutdownRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsCpu(MipsEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;
        RegisterFile = new(config.Version);
        CoProcessor0 = new();
        FloatProcessor = new();

        Tlb = new MipsTlb();
        Memory = new MemorySystem(bus, Tlb);

        // HOTFIX: Initialize $sp
        this[MipsGpRegister.StackPointer] = T.CreateTruncating(0x7FFF_8000);
    }

    /// <inheritdoc/>
    public string ArchitectureName => "MIPS";

    /// <inheritdoc/>
    public Endianness Endianness => Endianness.Big;

    /// <inheritdoc/>
    public MipsEmulatorConfig Config { get; }

    /// <inheritdoc cref="ICpu.ProgramCounter"/>
    public T ProgramCounter { get; set; }

    /// <inheritdoc/>
    ulong ICpu.ProgramCounter
    {
        get => ulong.CreateTruncating(ProgramCounter);
        set => ProgramCounter = T.CreateTruncating(value);
    }

    /// <inheritdoc cref="ICpu.RegisterFile"/>
    public MipsGPRegisterFile<T> RegisterFile { get; }

    /// <inheritdoc/>
    IRegisterFile ICpu.RegisterFile => RegisterFile;

    /// <summary>
    /// Gets the coprocessor 0 unit of the computer system.
    /// </summary>
    public CoProcessor0<T> CoProcessor0 { get; }

    /// <inheritdoc/>
    public FloatProcessor<T> FloatProcessor { get; }

    /// <inheritdoc/>
    IFloatProcessor IMipsCpu.FloatProcessor => FloatProcessor;

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    public MipsTlb Tlb { get; }

    /// <summary>
    /// Gets the system memory
    /// </summary>
    public MemorySystem Memory { get; }

    /// <inheritdoc/>
    public double ClockSpeed
    {
        get;
        protected set
        {
            field = value;
            Console.WriteLine($"Speed: {value / 1_000_000:F2} MHz");
        }
    }

    /// <inheritdoc cref="IMipsCpu.DelaySlot"/>
    public T? DelaySlot { get; protected set; }

    /// <inheritdoc/>
    ulong? IMipsCpu.DelaySlot => DelaySlot.HasValue
        ? ulong.CreateTruncating(DelaySlot.Value)
        : null ;

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public T this[MipsGpRegister reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <inheritdoc/>
    ulong IMipsCpu.this[MipsGpRegister reg]
    {
        get => ulong.CreateTruncating(RegisterFile[(int)reg]);
        set => RegisterFile[(int)reg] = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public abstract void Run(CancellationToken ct);

    /// <inheritdoc/>
    public abstract void Insert(MipsInstruction instruction, out MipsTrap trap);

    /// <inheritdoc/>
    public void RequestShutdown() => ShutdownRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Handles a trap.
    /// </summary>
    protected void HandleTrap(MipsTrap trap)
    {
        if (trap is MipsTrap.None)
            return;

        // Breakpoints are handled by the debugger upon the trap occurring event
        // The host also handles every kind of trap if that's what the config specifies
        if (trap is MipsTrap.Breakpoint && BreakpointHit is not null)
        {
            // Only wait if a debugger is attached
            var eventArgs = new BreakpointHitEventArgs();
            BreakpointHit.Invoke(this, eventArgs);
            eventArgs.Wait();
        }
        else if (Config.TrapHost is not null)
        {
            // The host handled the trap, do not emulate it
            // Breakpoints are always handled by the host
            Config.TrapHost.HandleTrap(new MipsTrapContext(this, (ulong)trap));
        }
        else
        {
            CoProcessor0.EnterTrap(trap, ProgramCounter, DelaySlot.HasValue);
            ProgramCounter = CoProcessor0.ExceptionVector;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        RegisterFile.Dispose();
        CoProcessor0.RegisterFile.Dispose();
        FloatProcessor.RegisterFile.Dispose();
    }
}
