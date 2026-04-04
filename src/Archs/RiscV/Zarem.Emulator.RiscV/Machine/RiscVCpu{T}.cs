// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public sealed partial class RiscVCpu<T> : IRiscVCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly IRiscVInstructionServiceTable<T> _instructionServiceTable;

    /// <inheritdoc/>
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVCpu(RiscVEmulatorConfig config, PhysicalBus bus)
    {
        Config = config;
        RegisterFile = new();
        Tlb = new RiscVTlb();
        Memory = new MemorySystem(bus, Tlb);

        _instructionServiceTable = config.VersionInfo.Base switch
        {
            RiscVBaseVersion.RV32 => new RiscVInstructionServiceTable<T, int>(this),
            RiscVBaseVersion.RV64 => new RiscVInstructionServiceTable<T, long>(this),
            RiscVBaseVersion.RV128 => new RiscVInstructionServiceTable<T, Int128>(this),
            _ => throw new NotImplementedException()
        };
    }

    /// <inheritdoc/>
    public string ArchitectureName => "RISC-V";

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

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public T this[GPRegister reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <inheritdoc/>
    ulong IRiscVCpu.this[GPRegister reg]
    {
        get => ulong.CreateTruncating(RegisterFile[(int)reg]);
        set => RegisterFile[(int)reg] = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        RegisterFile.Dispose();
    }
}
