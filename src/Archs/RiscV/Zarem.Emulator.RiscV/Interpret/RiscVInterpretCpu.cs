// Avishai Dernis 2026

using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models;
using Zarem.Emulator.TrapHandlers;
using Zarem.Models.Instructions;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Interpret;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public sealed class RiscVInterpretCpu<T> : RiscVCpu<T>, IInterpretCpu<RiscVInterpretCpu<T>, RiscVInstruction, RiscVExecution<T>, RiscVTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly IRiscVInstructionServiceTable<T> _instructionServiceTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVInterpretCpu(RiscVEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {

        _instructionServiceTable = config.VersionInfo.Base switch
        {
            RiscVBaseVersion.RV32 => new RiscVInstructionServiceTable<T, int>(this),
            RiscVBaseVersion.RV64 => new RiscVInstructionServiceTable<T, long>(this),
            RiscVBaseVersion.RV128 => new RiscVInstructionServiceTable<T, Int128>(this),
            _ => throw new NotImplementedException()
        };
    }

    /// <inheritdoc/>
    public override void Run(CancellationToken ct)
    {
        long totalInstructions = 0;
        var stopwatch = Stopwatch.StartNew();
        long lastReportTime = 0;

        while (!ct.IsCancellationRequested)
        {
            Step();

            // Update instruction count
            totalInstructions++;

            // Speed Check: Every 1000ms (1 second)
            long currentTime = stopwatch.ElapsedMilliseconds;
            if (currentTime - lastReportTime >= 1000)
            {
                double seconds = (currentTime - lastReportTime) / 1000.0;
                MeasuredSpeed = totalInstructions / seconds;

                // Reset for next interval
                totalInstructions = 0;
                lastReportTime = currentTime;
            }
        }
    }

    /// <inheritdoc/>
    public void Step()
    {
        // Fetch, Execute, and Apply the instruction
        var trap = Fetch(out var instruction);
        ExecuteAndApply(instruction, out _, trap);
    }

    /// <inheritdoc/>
    public override void Insert(RiscVInstruction instruction, out RiscVTrap trap)
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

        // Handle trap, if any occurred
        if (trap is not RiscVTrap.None)
            HandleTrap(trap);

        return trap;
    }

    private RiscVTrap Execute(RiscVInstruction instruction, out RiscVExecution<T> execution)
        => _instructionServiceTable.Execute(instruction, out execution);

    private RiscVTrap WriteBack(RiscVExecution<T> execution, T memRead)
    {
        T nextPc = ProgramCounter + T.CreateTruncating(4);

        // Handle gpr writeback
        RegisterFile[(int)execution.WritebackGPRegister] = execution.Writeback;

        switch (execution.SideEffect)
        {
            case SideEffect.ProgramCounter:
                nextPc = execution.ProgramCounter;
                break;
        }

        // Apply the program counter update
        ProgramCounter = nextPc;
        return RiscVTrap.None;
    }
}
