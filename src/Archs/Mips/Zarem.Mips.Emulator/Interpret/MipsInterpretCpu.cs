// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Interpret;

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
        _instructionServiceTable = config.VersionInfo.Is64Bit
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
        {
            execution = default;
            HandleTrap(trap);
        }

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
        int size = (int)execution.MemSize;

        MemoryAccessResult accessResult = MemoryAccessResult.Success;
        bool isWrite = false;

        if (execution.SideEffect is MipsSideEffect.ReadMemory or MipsSideEffect.ReadMemorySigned)
        {
            bool signed = execution.SideEffect is MipsSideEffect.ReadMemorySigned;

            switch (size)
            {
                case 1:
                    accessResult = Memory.TryRead(addr, out byte b);
                    read = signed ? T.CreateSaturating((sbyte)b) : T.CreateTruncating(b);
                    break;
                case 2:
                    accessResult = Memory.TryRead(addr, out ushort s);
                    read = signed ? T.CreateSaturating((short)s) : T.CreateTruncating(s);
                    break;
                case 4:
                    accessResult = Memory.TryRead(addr, out uint i);
                    read = signed ? T.CreateSaturating((int)i) : T.CreateTruncating(i);
                    break;
                case 8:
                    accessResult = Memory.TryRead(addr, out ulong l);
                    read = signed ? T.CreateSaturating((long)l) : T.CreateTruncating(l);
                    break;
                default:
                    return ThrowHelper.ThrowInvalidOperationException<MipsTrap>($"Invalid memory read size: {size}");
            }
        }
        else if (execution.SideEffect is MipsSideEffect.WriteMemory)
        {
            isWrite = true;
            accessResult = size switch
            {
                1 => Memory.TryWrite(addr, byte.CreateTruncating(execution.Writeback)),
                2 => Memory.TryWrite(addr, ushort.CreateTruncating(execution.Writeback)),
                4 => Memory.TryWrite(addr, uint.CreateTruncating(execution.Writeback)),
                8 => Memory.TryWrite(addr, ulong.CreateTruncating(execution.Writeback)),
                _ => throw new InvalidOperationException($"Invalid memory write size: {size}"),
            };
        }

        return accessResult switch
        {
            MemoryAccessResult.Success => MipsTrap.None,

            MemoryAccessResult.TranslationFault when isWrite => MipsTrap.TlbMissStore,
            MemoryAccessResult.TranslationFault => MipsTrap.TlbMissLoad,

            MemoryAccessResult.AddressError when isWrite => MipsTrap.AddressErrorStore,
            MemoryAccessResult.AddressError => MipsTrap.AddressErrorLoad,

            MemoryAccessResult.AccessViolation when isWrite => MipsTrap.AddressErrorStore,
            MemoryAccessResult.AccessViolation => MipsTrap.AddressErrorLoad,

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsTrap>(),
        };
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
            RegisterFile[(int)execution.WritebackGPRegister] = execution.Writeback;
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
                RegisterFile[(int)execution.WritebackGPRegister] = memRead;
                break;
            case MipsSideEffect.WriteCoProc0:
                CoProcessor0[execution.CoProc0Reg] = execution.CoProcWriteBack;
                break;
            case MipsSideEffect.WriteCoProc1Control:
                FloatProcessor.ControlRegisterFile[execution.CoProc1ControlReg] = execution.CoProc1ControlWriteBack;
                break;
            case MipsSideEffect.WriteSingle:
                FloatProcessor.Words[(int)execution.FloatReg] = execution.FWordWriteBack;
                break;
            case MipsSideEffect.WriteDouble:
                FloatProcessor.Longs[(int)execution.FloatReg] = execution.FLongWriteBack;
                break;
            case MipsSideEffect.TLBProbe:
                CoProcessor0.WritebackTlbp();
                break;
            case MipsSideEffect.TLBRead:
                CoProcessor0.WritebackTlbr();
                break;
            case MipsSideEffect.TLBWriteIndexed:
                CoProcessor0.WritebackTlbwi();
                break;
            case MipsSideEffect.TLBWriteRandom:
                CoProcessor0.WritebackTlbwr();
                break;
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
