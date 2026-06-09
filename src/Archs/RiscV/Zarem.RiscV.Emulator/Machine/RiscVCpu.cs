// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Machine.Registers;
using Zarem.Models.Enums;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Emulator.Machine.Registers;
using Zarem.RiscV.Emulator.TrapHandlers;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public abstract class RiscVCpu<T> : CpuBase<T>, IRiscVCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private const int FloatRegisterCount = 32;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVCpu(RiscVEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;
        RegisterFile = new();
        Tlb = new RiscVTlb();
        Memory = new MemorySystem(bus, Tlb);

        FloatRegisterFile = new FormattedRegisterFile<T>(FloatRegisterCount);

        //var extensions = Config.VersionInfo.Extensions;
        //if (extensions.HasFlag(RiscVExtensions.QuadrupleFloatingPoint)) FloatRegisterFile = new FormattedRegisterFile<UInt128>(FloatRegisterCount);
        //else if (extensions.HasFlag(RiscVExtensions.DoubleFloatingPoint)) FloatRegisterFile = new FormattedRegisterFile<ulong>(FloatRegisterCount);
        //else if (extensions.HasFlag(RiscVExtensions.SingleFloatingPoint)) FloatRegisterFile = new FormattedRegisterFile<uint>(FloatRegisterCount);
        //else if (extensions.HasFlag(RiscVExtensions.HalfPrecisionFloatingPoint)) FloatRegisterFile = new FormattedRegisterFile<ushort>(FloatRegisterCount); // This should be illegal, but best to be careful
    }

    /// <inheritdoc/>
    public override string ArchitectureName => "RISC-V";

    /// <inheritdoc/>
    public override Endianness Endianness => Endianness.Little;

    /// <inheritdoc/>
    public RiscVEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public override RiscVGPRegisterFile<T> RegisterFile { get; }

    /// <inheritdoc/>
    public IFormattedRegisterFile<T>? FloatRegisterFile { get; }

    /// <inheritdoc/>
    IFormattedRegisterFile? IRiscVCpu.FloatRegisterFile => FloatRegisterFile;

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    public RiscVTlb Tlb { get; }

    /// <inheritdoc/>
    public override MemorySystem Memory { get; }

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

    /// <summary>
    /// Handles a trap.
    /// </summary>
    protected void HandleTrap(RiscVTrap trap)
    {
        if (trap is RiscVTrap.None)
            return;

        // Breakpoints are handled by the debugger upon the trap occurring event
        // The host also handles every kind of trap if that's what the config specifies
        if (trap is RiscVTrap.Breakpoint && InvokeBreakpoint())
        {
            // Logic handled in InvokeBreakpoint
        }
        else
        {
            // The host handled the trap, do not emulate it
            // Breakpoints are always handled by the host
            Config.TrapHost?.HandleTrap(new RiscVTrapContext(this, (ulong)trap));
        }
    }
}
