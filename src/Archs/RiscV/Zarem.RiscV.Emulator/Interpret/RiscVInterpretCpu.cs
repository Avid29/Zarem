// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.Interpret;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override long ExecutionLoop()
    {
        Step();
        return 1;
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
        trap = trap is RiscVTrap.None ? MemAccess(execution, out memRead) : trap;
        trap = trap is RiscVTrap.None ? WriteBack(execution, memRead) : trap;

        // Handle trap, if any occurred
        if (trap is not RiscVTrap.None)
            HandleTrap(trap);

        return trap;
    }

    private RiscVTrap Execute(RiscVInstruction instruction, out RiscVExecution<T> execution)
        => _instructionServiceTable.Execute(instruction, out execution);

    private RiscVTrap MemAccess(RiscVExecution<T> execution, out T read)
    {
        read = default;

        ulong addr = ulong.CreateTruncating(execution.MemAddress);
        ulong size = execution.MemSize;

        // NOTE: Alignment was already checked during the execution phase.
        // No need to check it here too.

        if (execution.SideEffect is RiscVSideEffect.ReadMemory or RiscVSideEffect.ReadMemorySigned)
        {
            bool signed = execution.SideEffect is RiscVSideEffect.ReadMemorySigned;
            read = size switch
            {
                1 => signed ? T.CreateSaturating(Memory.Read<sbyte>(addr)) : T.CreateTruncating(Memory.Read<byte>(addr)),
                2 => signed ? T.CreateSaturating(Memory.Read<short>(addr)) : T.CreateTruncating(Memory.Read<ushort>(addr)),
                4 => signed ? T.CreateSaturating(Memory.Read<int>(addr)) : T.CreateTruncating(Memory.Read<uint>(addr)),
                8 => signed ? T.CreateSaturating(Memory.Read<long>(addr)) : T.CreateTruncating(Memory.Read<ulong>(addr)),
                _ => ThrowHelper.ThrowInvalidOperationException<T>($"Invalid memory read size: {size}"),
            };
        }
        else if (execution.SideEffect is RiscVSideEffect.WriteMemory)
        {
            switch (size)
            {
                case 1:
                    Memory.Write(addr, byte.CreateTruncating(execution.Writeback));
                    break;
                case 2:
                    Memory.Write(addr, ushort.CreateTruncating(execution.Writeback));
                    break;
                case 4:
                    Memory.Write(addr, uint.CreateTruncating(execution.Writeback));
                    break;
                case 8:
                    Memory.Write(addr, ulong.CreateTruncating(execution.Writeback));
                    break;
                default:
                    throw new InvalidOperationException($"Invalid memory write size: {size}");
            }
        }

        return RiscVTrap.None;
    }

    private RiscVTrap WriteBack(RiscVExecution<T> execution, T memRead)
    {
        T nextPc = ProgramCounter + T.CreateTruncating(4);

        // Handle gpr writeback
        RegisterFile[(int)execution.WritebackGPRegister] = execution.Writeback;

        switch (execution.SideEffect)
        {
            case RiscVSideEffect.ProgramCounter:
                nextPc = execution.ProgramCounter;
                break;
            case RiscVSideEffect.ReadMemory:
            case RiscVSideEffect.ReadMemorySigned:
                RegisterFile[(int)execution.WritebackGPRegister] = memRead;
                break;
        }

        // Apply the program counter update
        ProgramCounter = nextPc;
        return RiscVTrap.None;
    }
}
