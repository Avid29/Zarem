// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Events;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public sealed partial class RiscVCpu<T> : IRiscVCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
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
}
