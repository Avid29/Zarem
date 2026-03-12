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

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a processor unit.
/// </summary>
public partial class MipsCpu : ICpu<MipsCpu, MIPSInstruction, MIPSTrap>
{
    private int? _branchDelay = null;

    /// <inheritdoc/>
    public event EventHandler<MipsCpu, TrapEventArgs>? TrapOccurring;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu"/> class.
    /// </summary>
    public MipsCpu(IMemoryAccessor memory)
    {
        RegisterFile = new(true);
        CoProcessor0 = new ();
        FloatProcessor = new();
        Tlb = new MipsTlb();
        Memory = memory;
    }

    internal RegisterFile RegisterFile { get; }

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

    /// <inheritdoc/>
    public string ArchitectureName => "MIPS";

    /// <inheritdoc/>
    ulong ICpu.ProgramCounter
    {
        get => ProgramCounter;
        set => ProgramCounter = (uint)value;
    }

    /// <inheritdoc/>
    public void Step()
    {
        // Fetch, Execute, and Apply the instruction
        var trap = Fetch(out var instruction);
        ExecuteAndApply(instruction, out _, trap);
    }

    /// <inheritdoc/>
    public void Insert(MIPSInstruction instruction, out MIPSTrap trap)
        => Insert(instruction, out _, out trap);

    /// <inheritdoc cref="Insert(MIPSInstruction, out MIPSTrap)"/>
    public void Insert(MIPSInstruction instruction, out Execution execution, out MIPSTrap trap)
        => trap = ExecuteAndApply(instruction, out execution);

    /// <remarks>
    /// Immitates the fetch step in a MIPS cpu, reading an instruction from memory.
    /// </remarks>
    private MIPSTrap Fetch(out MIPSInstruction instruction)
    {
        instruction = default;

        if (ProgramCounter % 4 is not 0)
        {
            return MIPSTrap.AddressErrorLoad;
        }

        instruction = (MIPSInstruction)Computer.Memory.Read<uint>(ProgramCounter);
        return MIPSTrap.None;
    }

    /// <remarks>
    /// Wraps the last 3 stages of the instruction pipeline.
    /// This allows for executing instructions that were not fetched.
    /// </remarks>
    private MIPSTrap ExecuteAndApply(MIPSInstruction instruction, out Execution execution, MIPSTrap proceedingTrap = MIPSTrap.None)
    {
        // Pre-define everything to avoid unset variable accusations
        MIPSTrap trap = proceedingTrap;
        uint memRead = default;
        execution = default;

        // Perform the back-half of the MIPS pipeline
        trap = trap is MIPSTrap.None ? Execute(instruction, out execution) : trap;
        trap = trap is MIPSTrap.None ? MemAccess(execution, out memRead) : trap;
        trap = trap is MIPSTrap.None ? WriteBack(execution, memRead) : trap;

        // Handle trap, if any occurred
        if (trap is not MIPSTrap.None)
            HandleTrap(trap);

        return trap;
    }

    /// <summary>
    /// Immitates the execute step in a MIPS cpu, constructing the modifications to apply in the following stages.
    /// </summary>
    private MIPSTrap Execute(MIPSInstruction instruction, out Execution execution)
        => InstructionExecutor.Execute(instruction, this, out execution);

    private MIPSTrap MemAccess(Execution execution, out uint read)
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
                    ? (uint)Computer.Memory.Read<sbyte>(addr)
                    : Computer.Memory.Read<byte>(addr),
                2 => signed
                    ? (uint)Computer.Memory.Read<short>(addr)
                    : Computer.Memory.Read<ushort>(addr),
                4 => Computer.Memory.Read<uint>(addr),
                _ => ThrowHelper.ThrowInvalidOperationException<uint>($"Invalid memory read size: {size}"),
            };
        }
        else if (execution.SideEffect is SideEffect.WriteMemory)
        {
            switch (size)
            {
                case 1:
                    Computer.Memory.Write(addr, (byte)execution.WriteBack);
                    break;

                case 2:
                    Computer.Memory.Write(addr, (ushort)execution.WriteBack);
                    break;

                case 4:
                    Computer.Memory.Write(addr, execution.WriteBack);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid memory write size: {size}");
            }
        }

        return MIPSTrap.None;
    }

    private MIPSTrap WriteBack(Execution execution, uint memRead)
    {
        var programCounter = ProgramCounter;
        if (_branchDelay.HasValue)
        {
            var newPC = programCounter + _branchDelay.Value;
            programCounter = (uint)newPC;
            _branchDelay = null;
        }
        else
        {
            // Increment the program counter by default
            // (some instructions will override this)
            programCounter = ProgramCounter + 4;
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
            case SideEffect.JumpProgramCounter:
                programCounter = execution.ProgramCounter;
                break;
            case SideEffect.BranchProgramCounter:
                ApplyBranch(execution.Branch, ref programCounter);
                break;
            case SideEffect.ReadMemory:
                RegisterFile[execution.GPR] = memRead;
                break;
            case SideEffect.WriteCoProc:
                WriteCoProc(execution.CoProcRegisterSet, execution.CoProcReg, execution.CoProcWriteBack);
                break;
                // TODO: Handle TLB side effects
        }

        // Apply the program counter update
        ProgramCounter = programCounter;

        return MIPSTrap.None;
    }

    private void ApplyBranch(int branch, ref uint pc)
    {
        if (Computer.Config.DisableBranchDelays)
        {
            // Branch delays are disabled. Just change the PC
            pc = (uint)(pc + branch);
            return;
        }

        // Store the branch offset in the delay slot
        _branchDelay = branch;
    }

    private void WriteCoProc(RegisterSet set, GPRegister register, uint writeback)
    {
        var registerSet = set switch
        {
            RegisterSet.GeneralPurpose => RegisterFile,
            RegisterSet.CoProc0 => CoProcessor0.RegisterFile,
            RegisterSet.FloatingPoints => FloatProcessor.RegisterFile,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<RegisterFile>(nameof(set)),
        };

        registerSet[register] = writeback;
    }

    private void HandleTrap(MIPSTrap trap)
    {
        if (trap is MIPSTrap.None)
            return;

        // Breakpoints are handled by the debugger upon the trap occurring event
        // The host also handles every kind of trap if that's what the config specifies
        var hostTrap = trap is MIPSTrap.Breakpoint || Computer.Config.HostedTraps;
        var args = new TrapEventArgs((ulong)trap, hostTrap);
        TrapOccurring?.Invoke(this, args);

        // The host handled the trap, do not emulate it
        // Breakpoints are always handled by the host
        if (hostTrap)
        {
            // Wait for the host to handle the trap before resuming execution
            // Only do this if there's actually a host register to the even though
            if (TrapOccurring is not null)
                args.Wait();

            return;
        }

        // Status and cause registers
        CoProcessor0.StatusRegister = CoProcessor0.StatusRegister with { ExceptionLevel = true };
        CoProcessor0.CauseRegister = CoProcessor0.CauseRegister with
        {
            ExecptionCode = trap,
            //IsBranchDelayed = // TODO: Handle delay slots
        };

        // Track the current program counter in the EPC register
        // before jumping to the exception handler
        CoProcessor0[CP0Registers.ExceptionPC] = ProgramCounter;
        ProgramCounter = CoProcessor0.ExceptionVector;
    }
}
