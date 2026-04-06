// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Enum;
using Zarem.Emulator.TrapHandlers;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public sealed partial class MipsCpu<T> : IMipsCpu
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
    public void Insert(MipsInstruction instruction, out MipsTrap trap)
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

        if (execution.SideEffect is SideEffect.ReadMemory or SideEffect.ReadMemorySigned)
        {
            bool signed = execution.SideEffect is SideEffect.ReadMemorySigned;
            read = size switch
            {
                1 => signed ? T.CreateSaturating(Memory.Read<sbyte>(addr)) : T.CreateTruncating(Memory.Read<byte>(addr)),
                2 => signed ? T.CreateSaturating(Memory.Read<short>(addr)) : T.CreateTruncating(Memory.Read<ushort>(addr)),
                4 => signed ? T.CreateSaturating(Memory.Read<int>(addr)) : T.CreateTruncating(Memory.Read<uint>(addr)),
                8 => signed ? T.CreateSaturating(Memory.Read<long>(addr)) : T.CreateTruncating(Memory.Read<ulong>(addr)),
                _ => ThrowHelper.ThrowInvalidOperationException<T>($"Invalid memory read size: {size}"),
            };
        }
        else if (execution.SideEffect is SideEffect.WriteMemory)
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
        if (execution.SideEffect is not (SideEffect.ReadMemory or SideEffect.WriteMemory))
        {
            RegisterFile[(int)execution.GPR] = execution.WriteBack;
        }

        // Apply side effects
        switch (execution.SideEffect)
        {
            case SideEffect.Low:
                RegisterFile.Low = execution.Low;
                break;
            case SideEffect.High:
                RegisterFile.High = execution.High;
                break;
            case SideEffect.HighLow:
                (RegisterFile.High, RegisterFile.Low) = (execution.High, execution.Low);
                break;
            case SideEffect.ProgramCounter:
                ApplyJump(execution.ProgramCounter, ref nextPc);
                break;
            case SideEffect.ReadMemory:
            case SideEffect.ReadMemorySigned:
                RegisterFile[(int)execution.GPR] = memRead;
                break;
            case SideEffect.WriteCoProc0:
                CoProcessor0[execution.CoProc0Reg] = execution.CoProc0WriteBack;
                break;
            case SideEffect.WriteFloat:
                FloatProcessor.Words[execution.FloatReg] = execution.FWordWriteBack;
                break;
            case SideEffect.WriteDouble:
                FloatProcessor.Longs[execution.FloatReg] = execution.FLongWriteBack;
                break;
                // TODO: Handle TLB side effects
        }

        // Apply the program counter update
        ProgramCounter = nextPc;
        return MipsTrap.None;
    }

    private void ApplyJump(T targetPc, ref T nextPc)
    {
        if (Config.DisableDelaySlots)
        {
            // Branch delays are disabled. Just change the PC
            nextPc = targetPc;
            return;
        }

        // Store the branch offset in the delay slot
        DelaySlot = targetPc;
    }

    private void HandleTrap(MipsTrap trap)
    {
        if (trap is MipsTrap.None)
            return;

        // Breakpoints are handled by the debugger upon the trap occurring event
        // The host also handles every kind of trap if that's what the config specifies
        if (trap is MipsTrap.Breakpoint && BreakpointHit is not null)
        {
            // Only wait if a debugger is attached
            var eventArgs = new BreakpointHitEventArgs();
            BreakpointHit.Invoke(this, eventArgs);
            eventArgs.Wait();
        }
        else if (Config.TrapHost is not null)
        {
            // The host handled the trap, do not emulate it
            // Breakpoints are always handled by the host
            Config.TrapHost.HandleTrap(new MipsTrapContext(this, (ulong)trap));
        }
        else
        {
            CoProcessor0.EnterTrap(trap, ProgramCounter, DelaySlot.HasValue);
            ProgramCounter = CoProcessor0.ExceptionVector;
        }
    }
}
