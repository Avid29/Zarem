// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models;
using Zarem.Extensions;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Interpret;

/// <summary>
/// A <see cref="MipsCpu{T}"/> that executes by interpreting each instruction.
/// </summary>
public sealed class MipsInterpretCpu<T> : MipsCpu<T>, IInterpretCpu<MipsInterpretCpu<T>, MipsInstruction, MipsExecution<T>, MipsTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly IMipsInstructionServiceTable<T> _instructionServiceTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsInterpretCpu(MipsEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _instructionServiceTable = config.Version.Is64Bit()
            ? new MipsInstructionServiceTable<T, long>(this)
            : new MipsInstructionServiceTable<T, int>(this);
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
    public override void Insert(MipsInstruction instruction, out MipsTrap trap)
        => Insert(instruction, out _, out trap);

    /// <inheritdoc cref="ICpu{TSelf, TInstruction, TTrap}.Insert(TInstruction, out TTrap)"/>
    public void Insert(MipsInstruction instruction, out MipsExecution<T> execution, out MipsTrap trap)
        => trap = ExecuteAndApply(instruction, out execution);

    /// <remarks>
    /// Immitates the fetch step in a MIPS cpu, reading an instruction from memory.
    /// </remarks>
    private MipsTrap Fetch(out MipsInstruction instruction)
    {
        instruction = default;

        if (ProgramCounter % T.CreateTruncating(4) != T.Zero)
            return MipsTrap.AddressErrorLoad;

        instruction = (MipsInstruction)Memory.Read<uint>(ulong.CreateTruncating(ProgramCounter));
        return MipsTrap.None;
    }

    /// <remarks>
    /// Wraps the last 3 stages of the instruction pipeline.
    /// This allows for executing instructions that were not fetched.
    /// </remarks>
    private MipsTrap ExecuteAndApply(MipsInstruction instruction, out MipsExecution<T> execution, MipsTrap proceedingTrap = MipsTrap.None)
    {
        // Pre-define everything to avoid unset variable accusations
        MipsTrap trap = proceedingTrap;
        T memRead = default;
        execution = default;

        // Perform the back-half of the MIPS pipeline
        trap = trap is MipsTrap.None ? Execute(instruction, out execution) : trap;
        trap = trap is MipsTrap.None ? MemAccess(execution, out memRead) : trap;
        trap = trap is MipsTrap.None ? WriteBack(execution, memRead) : trap;

        // Handle trap, if any occurred
        if (trap is not MipsTrap.None)
            HandleTrap(trap);

        return trap;
    }

    /// <summary>
    /// Immitates the execute step in a MIPS cpu, constructing the modifications to apply in the following stages.
    /// </summary>
    private MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
        => _instructionServiceTable.Execute(instruction, out execution);

    private MipsTrap MemAccess(MipsExecution<T> execution, out T read)
    {
        read = default;

        ulong addr = ulong.CreateTruncating(execution.MemAddress);
        ulong size = execution.MemSize;

        // NOTE: Alignment was already checked during the execution phase.
        // No need to check it here too.

        if (execution.SideEffect is MipsSideEffect.ReadMemory or MipsSideEffect.ReadMemorySigned)
        {
            bool signed = execution.SideEffect is MipsSideEffect.ReadMemorySigned;
            read = size switch
            {
                1 => signed ? T.CreateSaturating(Memory.Read<sbyte>(addr)) : T.CreateTruncating(Memory.Read<byte>(addr)),
                2 => signed ? T.CreateSaturating(Memory.Read<short>(addr)) : T.CreateTruncating(Memory.Read<ushort>(addr)),
                4 => signed ? T.CreateSaturating(Memory.Read<int>(addr)) : T.CreateTruncating(Memory.Read<uint>(addr)),
                8 => signed ? T.CreateSaturating(Memory.Read<long>(addr)) : T.CreateTruncating(Memory.Read<ulong>(addr)),
                _ => ThrowHelper.ThrowInvalidOperationException<T>($"Invalid memory read size: {size}"),
            };
        }
        else if (execution.SideEffect is MipsSideEffect.WriteMemory)
        {
            switch (size)
            {
                case 1:
                    Memory.Write(addr, byte.CreateTruncating(execution.WriteBack));
                    break;
                case 2:
                    Memory.Write(addr, ushort.CreateTruncating(execution.WriteBack));
                    break;
                case 4:
                    Memory.Write(addr, uint.CreateTruncating(execution.WriteBack));
                    break;
                case 8:
                    Memory.Write(addr, ulong.CreateTruncating(execution.WriteBack));
                    break;
                default:
                    throw new InvalidOperationException($"Invalid memory write size: {size}");
            }
        }

        return MipsTrap.None;
    }

    private MipsTrap WriteBack(MipsExecution<T> execution, T memRead)
    {
        // Calculate what the next pc will be.
        // If a previous instruction set a DelaySlot, we go there.
        // Otherwise, we move forward.
        T nextPc = DelaySlot ?? (ProgramCounter + T.CreateTruncating(4));
        DelaySlot = null;

        // Handle gpr writeback
        if (execution.SideEffect is not (MipsSideEffect.ReadMemory or MipsSideEffect.WriteMemory))
        {
            RegisterFile[(int)execution.GPR] = execution.WriteBack;
        }

        // Apply side effects
        switch (execution.SideEffect)
        {
            case MipsSideEffect.Low:
                RegisterFile.Low = execution.Low;
                break;
            case MipsSideEffect.High:
                RegisterFile.High = execution.High;
                break;
            case MipsSideEffect.HighLow:
                (RegisterFile.High, RegisterFile.Low) = (execution.High, execution.Low);
                break;
            case MipsSideEffect.ProgramCounter:
            case MipsSideEffect.ForceProgramCounter:
                ApplyJump(execution.ProgramCounter, ref nextPc, execution.SideEffect is MipsSideEffect.ForceProgramCounter);
                break;
            case MipsSideEffect.ReadMemory:
            case MipsSideEffect.ReadMemorySigned:
                RegisterFile[(int)execution.GPR] = memRead;
                break;
            case MipsSideEffect.WriteCoProc0:
                CoProcessor0[execution.CoProc0Reg] = execution.CoProc0WriteBack;
                break;
            case MipsSideEffect.WriteFloat:
                FloatProcessor.Words[execution.FloatReg] = execution.FWordWriteBack;
                break;
            case MipsSideEffect.WriteDouble:
                FloatProcessor.Longs[execution.FloatReg] = execution.FLongWriteBack;
                break;
                // TODO: Handle TLB side effects
        }

        // Apply the program counter update
        ProgramCounter = nextPc;
        return MipsTrap.None;
    }

    private void ApplyJump(T targetPc, ref T nextPc, bool force)
    {
        if (force || Config.DisableDelaySlots)
        {
            // Branch delays are disabled. Just change the PC
            nextPc = targetPc;
            return;
        }

        // Store the branch offset in the delay slot
        DelaySlot = targetPc;
    }
}
