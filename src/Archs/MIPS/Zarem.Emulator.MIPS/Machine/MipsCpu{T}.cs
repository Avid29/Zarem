// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Enum;
using Zarem.Extensions;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public partial class MipsCpu<T> : MipsCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly InstructionServiceTable<T> _instructionServiceTable;
    private T? _delaySlot;

    /// <inheritdoc/>
    public override event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu"/> class.
    /// </summary>
    public MipsCpu(MIPSEmulatorConfig config, IMemoryAccessor memory) : base(config, memory)
    {
        RegisterFile = new(config.MipsVersion);
        CoProcessor0 = new();
        FloatProcessor = new();

        _instructionServiceTable = config.MipsVersion.Is64Bit()
            ? new InstructionServiceTable<T, long>(this)
            : new InstructionServiceTable<T, int>(this);
    }

    /// <inheritdoc/>
    public override MipsGpRegisterFile<T> RegisterFile { get; }

    /// <inheritdoc cref="ProgramCounter"/>
    public T PC { get; set; }

    /// <inheritdoc/>
    public override ulong ProgramCounter
    {
        get => ulong.CreateTruncating(PC);
        set => PC = T.CreateTruncating(value);
    }

    /// <summary>
    /// Gets the coprocessor 0 unit of the computer system.
    /// </summary>
    public CoProcessor0<T> CoProcessor0 { get; }

    /// <inheritdoc/>
    public override FloatProcessor<T> FloatProcessor { get; }

    /// <summary>
    /// Gets the jump address in the delay slot.
    /// </summary>
    public override ulong? DelaySlot => _delaySlot.HasValue
        ? ulong.CreateTruncating(_delaySlot.Value)
        : null ;

    /// <inheritdoc cref="this[int]"/>
    public T this[GPRegister reg]
    {
        get => RegisterFile[(int)reg];
        set => RegisterFile[(int)reg] = value;
    }

    /// <inheritdoc/>
    public override ulong this[int reg]
    {
        get => ulong.CreateTruncating(RegisterFile[reg]);
        set => RegisterFile[reg] = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public override void Step()
    {
        // Fetch, Execute, and Apply the instruction
        var trap = Fetch(out var instruction);
        ExecuteAndApply(instruction, out _, trap);
    }

    /// <inheritdoc/>
    public override void Insert(MipsInstruction instruction, out MipsTrap trap)
        => Insert(instruction, out _, out trap);

    /// <inheritdoc cref="MipsCpu.Insert(MipsInstruction, out MipsTrap)"/>
    public void Insert(MipsInstruction instruction, out Execution<T> execution, out MipsTrap trap)
        => trap = ExecuteAndApply(instruction, out execution);

    /// <remarks>
    /// Immitates the fetch step in a MIPS cpu, reading an instruction from memory.
    /// </remarks>
    private MipsTrap Fetch(out MipsInstruction instruction)
    {
        instruction = default;

        if (PC % T.CreateTruncating(4) != T.Zero)
            return MipsTrap.AddressErrorLoad;

        instruction = (MipsInstruction)Memory.Read<uint>(ProgramCounter);
        return MipsTrap.None;
    }

    /// <remarks>
    /// Wraps the last 3 stages of the instruction pipeline.
    /// This allows for executing instructions that were not fetched.
    /// </remarks>
    private MipsTrap ExecuteAndApply(MipsInstruction instruction, out Execution<T> execution, MipsTrap proceedingTrap = MipsTrap.None)
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
    private MipsTrap Execute(MipsInstruction instruction, out Execution<T> execution)
        => _instructionServiceTable.Execute(instruction, out execution);

    private MipsTrap MemAccess(Execution<T> execution, out T read)
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

    private MipsTrap WriteBack(Execution<T> execution, T memRead)
    {
        // Calculate what the next pc will be.
        // If a previous instruction set a DelaySlot, we go there.
        // Otherwise, we move forward.
        T nextPc = _delaySlot ?? (PC + T.CreateTruncating(4));
        _delaySlot = null;

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
        PC = nextPc;
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
        _delaySlot = targetPc;
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
            Config.TrapHost.HandleTrap(this, (ulong)trap);
        }
        else
        {
            CoProcessor0.EnterTrap(trap, PC, DelaySlot.HasValue);
            PC = CoProcessor0.ExceptionVector;
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        RegisterFile.Dispose();
        CoProcessor0.RegisterFile.Dispose();
        FloatProcessor.RegisterFile.Dispose();
    }
}
