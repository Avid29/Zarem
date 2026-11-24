// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System.Numerics;
using System.Transactions;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Memory;
using Zarem.Mips.Emulator.Machine.Memory.Tlb;
using Zarem.Mips.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.TrapHandlers;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Mips.Models.Versioning.Enums;
using Zarem.Models.Enums;

namespace Zarem.Mips.Emulator.Machine;

/// <summary>
/// A base class representing a processor unit.
/// </summary>
public partial class MipsCpu<T> : CpuBase<T>, IMipsCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// The boot address for a MIPS CPU.
    /// </summary>
    public const ulong BOOT_ADDRESS = 0xBFC0_0000;

    private readonly IExecutor<MipsInstruction, MipsTrap> _executor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsCpu(MipsEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;
        RegisterFile = new(config.VersionInfo);

        var coProcessorCount = config.VersionInfo.Generation is < MipsGeneration.MipsIII ? 3 : 2;
        CoProcessors = new ICoProcessor<T>[coProcessorCount];

        var tlb = new MipsTlb<T>();
        ControlCoProcessor = new(tlb);
        FloatCoProcessor = config.VersionInfo.Generation is >= MipsGeneration.MipsIII
            ? new FloatCoProcessor<T, ulong>(this)
            : new FloatCoProcessor<T, uint>(this);

        CoProcessors[1] = FloatCoProcessor;

        Memory = new MemorySystem(bus, new MipsAddressTranslator<T>(ControlCoProcessor));
        ProgramCounter = T.CreateTruncating(BOOT_ADDRESS);

        _executor = Config.ExecutionMode switch
        {
            ExecutionMode.Interpret => new MipsInterpretedExecutor(this),
            ExecutionMode.JustInTime => new MipsJitExecutor(this, bus),
            _ => ThrowHelper.ThrowArgumentException<IExecutor<MipsInstruction, MipsTrap>>(),
        };
    }

    /// <inheritdoc/>
    public override string ArchitectureName => "MIPS";

    /// <inheritdoc/>
    public override Endianness Endianness => Endianness.Big;

    /// <inheritdoc/>
    public MipsEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public override MipsGPRegisterFile<T> RegisterFile { get; }

    /// <inheritdoc/>
    public ControlCoProcessor<T> ControlCoProcessor { get; }

    /// <inheritdoc/>
    public IFloatCoProcessor<T> FloatCoProcessor { get; }

    /// <inheritdoc/>
    public ICoProcessor<T>[] CoProcessors { get; }

    /// <summary>
    /// Gets the system memory.
    /// </summary>
    public override MemorySystem Memory { get; }

    /// <inheritdoc cref="IMipsCpu.DelaySlot"/>
    public T? DelaySlot { get; protected set; }

    /// <inheritdoc/>
    ulong? IMipsCpu.DelaySlot => DelaySlot.HasValue
        ? ulong.CreateTruncating(DelaySlot.Value)
        : null;

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

    ulong IMipsCpu.this[MipsGpRegister reg]
    {
        get => ulong.CreateTruncating(RegisterFile[(int)reg]);
        set => RegisterFile[(int)reg] = T.CreateTruncating(value);
    }

    IControlCoProcessor? IMipsCpu.ControlCoProcessor => ControlCoProcessor;

    ICoProcessor[] IMipsCpu.CoProcessors => CoProcessors;

    IFloatCoProcessor? IMipsCpu.FloatCoProcessor => FloatCoProcessor;

    /// <inheritdoc/>
    protected override long ExecutionLoop()
        => _executor.ExecutionLoop();

    /// <inheritdoc/>
    public void Insert(MipsInstruction instruction, out MipsTrap trap)
        => _executor.Insert(instruction, out trap);

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
            ControlCoProcessor.EnterTrap(trap, ProgramCounter, DelaySlot.HasValue);
            ProgramCounter = ControlCoProcessor.ExceptionVector;
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        base.Dispose();
        ControlCoProcessor.RegisterFile.Dispose();
    }
}
