// Avishai Dernis 2025

using System;
using System.Numerics;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models;
using Zarem.Extensions;
using Zarem.Models.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public sealed partial class MipsCpu<T> : IMipsCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly IMipsInstructionServiceTable<T> _instructionServiceTable;

    /// <inheritdoc/>
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <inheritdoc/>
    public event EventHandler? ShutdownRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsCpu(MIPSEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;
        RegisterFile = new(config.Version);
        CoProcessor0 = new();
        FloatProcessor = new();

        Tlb = new MipsTlb();
        Memory = new MemorySystem(bus, Tlb);

        _instructionServiceTable = config.Version.Is64Bit()
            ? new MipsInstructionServiceTable<T, long>(this)
            : new MipsInstructionServiceTable<T, int>(this);

        // HOTFIX: Initialize $sp
        this[MipsGpRegister.StackPointer] = T.CreateTruncating(0x7FFF_8000);
    }

    /// <inheritdoc/>
    public string ArchitectureName => "MIPS";

    /// <inheritdoc/>
    public Endianness Endianness => Endianness.Big;

    /// <inheritdoc/>
    public MIPSEmulatorConfig Config { get; }

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

    /// <inheritdoc cref="IMipsCpu.DelaySlot"/>
    public T? DelaySlot { get; private set; }

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
    public void RequestShutdown() => ShutdownRequested?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    public void Dispose()
    {
        RegisterFile.Dispose();
        CoProcessor0.RegisterFile.Dispose();
        FloatProcessor.RegisterFile.Dispose();
    }
}
