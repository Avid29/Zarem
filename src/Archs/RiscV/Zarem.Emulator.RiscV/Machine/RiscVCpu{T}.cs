// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public class RiscVCpu<T> : IRiscVCpu
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
    public RiscVEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public T ProgramCounter { get; set; }

    /// <inheritdoc/>
    public RiscVGPRegisterFile<T> RegisterFile { get; }

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
    public string ArchitectureName => "RISC-V";

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    public RiscVTlb Tlb { get; }

    /// <inheritdoc/>
    public IMemorySystem Memory { get; }

    /// <inheritdoc/>
    ulong ICpu.ProgramCounter
    {
        get => ulong.CreateTruncating(ProgramCounter);
        set => ProgramCounter = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public void Step()
    {
        // Fetch, Execute, and Apply the instruction
        var trap = Fetch(out var instruction);
        ExecuteAndApply(instruction, out _, trap);
    }

    /// <inheritdoc/>
    public void Insert(RiscVInstruction instruction, out RiscVTrap trap)
        => Insert(instruction, out _, out trap);

    /// <inheritdoc/>
    public void Insert(RiscVInstruction instruction, out RiscVExecution<T> execution, out RiscVTrap trap)
        => trap = ExecuteAndApply(instruction, out execution);

    private RiscVTrap Fetch(out RiscVInstruction instruction)
    {
        instruction = default;

        if (ProgramCounter % T.CreateTruncating(4) != T.Zero)
            return RiscVTrap.InstructionAddressMisaligned;

        instruction = (RiscVInstruction)Memory.Read<uint>(ulong.CreateTruncating(ProgramCounter));
        return RiscVTrap.None;
    }

    private RiscVTrap ExecuteAndApply(RiscVInstruction instruction, out RiscVExecution<T> execution, RiscVTrap proceedingTrap = RiscVTrap.None)
    {
        RiscVTrap trap = proceedingTrap;
        T memRead = default;
        execution = default;

        trap = trap is RiscVTrap.None ? Execute(instruction, out execution) : trap;
        //trap = trap is RiscVTrap.None ? MemAccess(execution, out memRead) : trap;
        trap = trap is RiscVTrap.None ? WriteBack(execution, memRead) : trap;

        if (trap is RiscVTrap.Breakpoint)
        {
            BreakpointHit?.Invoke(this, new BreakpointHitEventArgs());
        }

        return trap;
    }

    private RiscVTrap Execute(RiscVInstruction instruction, out RiscVExecution<T> execution)
        => _instructionServiceTable.Execute(instruction, out execution);

    private RiscVTrap WriteBack(RiscVExecution<T> execution, T memRead)
    {
        T nextPc = ProgramCounter + T.CreateTruncating(4);

        // Handle gpr writeback
        RegisterFile[(int)execution.WritebackGPRegister] = execution.Writeback;

        // Apply the program counter update
        ProgramCounter = nextPc;
        return RiscVTrap.None;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        RegisterFile.Dispose();
    }
}
