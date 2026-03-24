// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System;
using Zarem.Emulator.Events;
using Zarem.Emulator.Executor;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Config;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public partial class MipsCpu : ICpu<MipsCpu, MipsInstruction, MipsTrap>
{
    private InstructionServiceTable _instructionServiceTable;

    /// <inheritdoc/>
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    internal event EventHandler? ShutdownRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu"/> class.
    /// </summary>
    public MipsCpu(MIPSEmulatorConfig config, IMemoryAccessor memory)
    {
        Config = config;

        RegisterFile = new(RegisterSet.GeneralPurpose);
        CoProcessor0 = new ();
        FloatProcessor = new();
        Tlb = new MipsTlb();
        Memory = memory;

        _instructionServiceTable = new InstructionServiceTable(this);
    }

    /// <summary>
    /// Gets the cpu's general purpose register file.
    /// </summary>
    public MipsRegisterFile RegisterFile { get; }

    /// <summary>
    /// Gets or sets the value in the program counter register.
    /// </summary>
    public uint ProgramCounter { get; set; }

    /// <summary>
    /// Gets the coprocessor 0 unit of the computer system.
    /// </summary>
    public CoProcessor0 CoProcessor0 { get; }

    /// <summary>
    /// Gets the floating-point coprocessor of the computer system.
    /// </summary>
    public FloatProcessor FloatProcessor { get; }

    /// <summary>
    /// Gets the translation look-aside buffer.
    /// </summary>
    public MipsTlb Tlb { get; }

    /// <summary>
    /// Gets the emulation config.
    /// </summary>
    public MIPSEmulatorConfig Config { get; }

    /// <summary>
    /// Gets the system memory
    /// </summary>
    public IMemoryAccessor Memory { get; internal set; }

    /// <summary>
    /// Gets or sets the value of a general-purpose register on the processor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public uint this[GPRegister reg]
    {
        get => RegisterFile[reg];
        set => RegisterFile[reg] = value;
    }

    /// <summary>
    /// Gets or sets the value in the low register.
    /// </summary>
    public uint Low { get; set; }

    /// <summary>
    /// Gets or sets the value in the high register.
    /// </summary>
    public uint High { get; set; }

    /// <summary>
    /// Gets the jump address in the delay slot.
    /// </summary>
    public uint? DelaySlot { get; private set; } = null;

    /// <inheritdoc/>
    public string ArchitectureName => "MIPS";

    /// <inheritdoc/>
    ulong ICpu.ProgramCounter
    {
        get => ProgramCounter;
        set => ProgramCounter = (uint)value;
    }

    /// <summary>
    /// Requests a shutdown.
    /// </summary>
    public void RequestShutdown() => ShutdownRequested?.Invoke(this, EventArgs.Empty);

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

    /// <inheritdoc cref="Insert(MipsInstruction, out MipsTrap)"/>
    public void Insert(MipsInstruction instruction, out Execution execution, out MipsTrap trap)
        => trap = ExecuteAndApply(instruction, out execution);

    /// <remarks>
    /// Immitates the fetch step in a MIPS cpu, reading an instruction from memory.
    /// </remarks>
    private MipsTrap Fetch(out MipsInstruction instruction)
    {
        instruction = default;

        if (ProgramCounter % 4 is not 0)
        {
            return MipsTrap.AddressErrorLoad;
        }

        instruction = (MipsInstruction)Memory.Read<uint>(ProgramCounter);
        return MipsTrap.None;
    }

    /// <remarks>
    /// Wraps the last 3 stages of the instruction pipeline.
    /// This allows for executing instructions that were not fetched.
    /// </remarks>
    private MipsTrap ExecuteAndApply(MipsInstruction instruction, out Execution execution, MipsTrap proceedingTrap = MipsTrap.None)
    {
        // Pre-define everything to avoid unset variable accusations
        MipsTrap trap = proceedingTrap;
        uint memRead = default;
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
    private MipsTrap Execute(MipsInstruction instruction, out Execution execution)
        => _instructionServiceTable.Execute(instruction, out execution);

    private MipsTrap MemAccess(Execution execution, out uint read)
    {
        read = default;

        uint addr = execution.MemAddress;
        uint size = execution.MemSize;
        bool signed = execution.MemSigned;

        // NOTE: Alignment was already checked during the execution phase.
        // No need to check it here too.

        if (execution.SideEffect is SideEffect.ReadMemory)
        {
            read = size switch
            {
                1 => signed
                    ? (uint)Memory.Read<sbyte>(addr)
                    : Memory.Read<byte>(addr),
                2 => signed
                    ? (uint)Memory.Read<short>(addr)
                    : Memory.Read<ushort>(addr),
                4 => Memory.Read<uint>(addr),
                _ => ThrowHelper.ThrowInvalidOperationException<uint>($"Invalid memory read size: {size}"),
            };
        }
        else if (execution.SideEffect is SideEffect.WriteMemory)
        {
            switch (size)
            {
                case 1:
                    Memory.Write(addr, (byte)execution.WriteBack);
                    break;

                case 2:
                    Memory.Write(addr, (ushort)execution.WriteBack);
                    break;

                case 4:
                    Memory.Write(addr, execution.WriteBack);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid memory write size: {size}");
            }
        }

        return MipsTrap.None;
    }

    private MipsTrap WriteBack(Execution execution, uint memRead)
    {
        uint nextPc;
        if (DelaySlot.HasValue)
        {
            nextPc = DelaySlot.Value;
            DelaySlot = null;
        }
        else
        {
            // Increment the program counter by default
            // (some instructions will override this)
            nextPc = ProgramCounter + 4;
        }

        // Handle gpr writeback
        // NOTE: This will clear the register momentarily during load operations.
        RegisterFile[execution.GPR] = execution.WriteBack;

        // Apply side effects
        switch (execution.SideEffect)
        {
            case SideEffect.Low:
                Low = execution.Low;
                break;
            case SideEffect.High:
                High = execution.High;
                break;
            case SideEffect.HighLow:
                (High, Low) = (execution.High, execution.Low);
                break;
            case SideEffect.ProgramCounter:
                ApplyJump(execution.ProgramCounter, ref nextPc);
                break;
            case SideEffect.ReadMemory:
                RegisterFile[execution.GPR] = memRead;
                break;
            case SideEffect.WriteCoProc:
                WriteCoProc(execution.CoProcRegisterSet, execution.CoProcReg, execution.CoProcWriteBack);
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

    private void ApplyJump(uint targetPc, ref uint nextPc)
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

    private void WriteCoProc(RegisterSet set, GPRegister register, uint writeback)
    {
        var registerSet = set switch
        {
            RegisterSet.GeneralPurpose => RegisterFile,
            RegisterSet.CoProc0 => CoProcessor0.RegisterFile,
            RegisterSet.FloatingPoints => FloatProcessor.RegisterFile,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsRegisterFile>(nameof(set)),
        };

        registerSet[register] = writeback;
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
            CoProcessor0.EnterTrap(trap, ProgramCounter, DelaySlot.HasValue);
            ProgramCounter = CoProcessor0.ExceptionVector;
        }
    }
}
