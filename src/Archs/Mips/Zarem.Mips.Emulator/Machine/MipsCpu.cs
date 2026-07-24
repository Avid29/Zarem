// Avishai Dernis 2025

using System.Numerics;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.Machine.Tlb;
using Zarem.Mips.Emulator.TrapHandlers;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Models.Enums;

namespace Zarem.Mips.Emulator.Machine;

/// <summary>
/// A base class representing a processor unit.
/// </summary>
public abstract partial class MipsCpu<T> : CpuBase<T>, IMipsCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsCpu(MipsEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;
        RegisterFile = new(config.VersionInfo);
        CoProcessor0 = new();
        FloatProcessor = new();

        Tlb = new MipsTlb();
        Memory = new MemorySystem(bus, Tlb);

        // HOTFIX: Initialize $sp
        this[MipsGpRegister.StackPointer] = T.CreateTruncating(0x7FFF_8000);
    }

    /// <inheritdoc/>
    public override string ArchitectureName => "MIPS";

    /// <inheritdoc/>
    public override Endianness Endianness => Endianness.Big;

    /// <inheritdoc/>
    public MipsEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public override MipsGPRegisterFile<T> RegisterFile { get; }

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
    /// Gets the system memory.
    /// </summary>
    public override MemorySystem Memory { get; }

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
    public abstract void Insert(MipsInstruction instruction, out MipsTrap trap);

    /// <summary>
    /// Handles a trap.
    /// </summary>
    protected void HandleTrap(MipsTrap trap)
    {
        if (trap is MipsTrap.None)
            return;

        // Breakpoints are handled by the debugger upon the trap occurring event
        // The host also handles every kind of trap if that's what the config specifies
        if (trap is MipsTrap.Breakpoint && InvokeBreakpoint())
        {
            // Logic handled in InvokeBreakpoint
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
    public override void Dispose()
    {
        base.Dispose();
        CoProcessor0.RegisterFile.Dispose();
        FloatProcessor.RegisterFile.Dispose();
    }
}
