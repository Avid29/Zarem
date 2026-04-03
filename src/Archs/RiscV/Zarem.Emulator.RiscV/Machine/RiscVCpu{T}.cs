// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public class RiscVCpu<T> : IRiscVCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVCpu(RiscVEmulatorConfig config, IMemoryAccessor memory)
    {
        Config = config;
        Memory = memory;
    }

    /// <inheritdoc/>
    public RiscVEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public T ProgramCounter { get; set; }

    /// <inheritdoc/>
    public string ArchitectureName => "RISC-V";

    /// <inheritdoc/>
    public IMemoryAccessor Memory { get; set; }

    /// <inheritdoc/>
    ulong ICpu.ProgramCounter
    {
        get => ulong.CreateTruncating(ProgramCounter);
        set => ProgramCounter = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public void Insert(RiscVInstruction instruction, out RiscVTrap trap)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Step()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
