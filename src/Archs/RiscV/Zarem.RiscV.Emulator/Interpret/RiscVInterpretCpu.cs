// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Enums;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.Interpret;

/// <summary>
/// A class representing a RISC-V CPU.
/// </summary>
public class RiscVInterpretCpu<T, TFloat> : RiscVCpu<T, TFloat>, IInterpretCpu<RiscVInterpretCpu<T, TFloat>, RiscVInstruction, RiscVExecution<T>, RiscVTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private readonly IRiscVInstructionServiceTable<T> _instructionServiceTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVInterpretCpu(RiscVEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _instructionServiceTable = config.VersionInfo.Base switch
        {
            RiscVBaseVersion.RV32 => new RiscVInstructionServiceTable<T, TFloat, int>(this),
            RiscVBaseVersion.RV64 => new RiscVInstructionServiceTable<T, TFloat, long>(this),
            RiscVBaseVersion.RV128 => new RiscVInstructionServiceTable<T, TFloat, Int128>(this),
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

        var isCompressedEnabled = Config.VersionInfo.HasExtensions(RiscVExtensions.Compressed);
        var alignmentMask = T.CreateTruncating(isCompressedEnabled ? 0b01 : 0b11);
        if ((ProgramCounter & alignmentMask) != T.Zero)
            return RiscVTrap.InstructionAddressMisaligned;

        var pc = ulong.CreateTruncating(ProgramCounter);

        // TODO: Handle memory exceptions in fetch

        // If compressed isn't present we can just fetch the 32-bit instruction ez-pz
        if (!isCompressedEnabled)
        {
            instruction = (RiscVInstruction)Memory.Read<uint>(pc);
            return RiscVTrap.None;
        }

        if ((pc & 0b11) is 0)
        {
            // RVC Fast Path: We're 4-byte aligned, so no need to worry about crossing a page boundary
            instruction = (RiscVInstruction)Memory.Read<uint>(pc);
            if (instruction.IsCompressed)
            {
                // Trim the instruction to the compressed instruction if compressed
                instruction = (RiscVCompressedInstruction)instruction;
            }

            return RiscVTrap.None;
        }
        else
        {
            // RVC Slow Path: Read the instruction 2-bytes at a time in case reading a compressed instruction
            // at the end of a page boundary

            var parcel0 = Memory.Read<ushort>(pc);
            var compressedInstruction = (RiscVCompressedInstruction)parcel0;
            if (compressedInstruction.CompressionCode is not RiscVCompressionCode.Uncompressed)
            {
                // The instruction is a compressed instruction
                // Cast to full instruction and return
                instruction = compressedInstruction;
                return RiscVTrap.None;
            }
            else
            {
                // Fetch second half and return
                var parcel1 = Memory.Read<ushort>(pc + 2);
                instruction = (RiscVInstruction)(((uint)parcel1 << 16) | parcel0);
                return RiscVTrap.None;
            }
        }
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
            case RiscVSideEffect.WriteHalf:
                FloatRegisterFile?.Halves[(int)execution.FloatReg] = execution.HalfWriteBack;
                break;
            case RiscVSideEffect.WriteSingle:
                FloatRegisterFile?.Singles[(int)execution.FloatReg] = execution.SingleWriteBack;
                break;
            case RiscVSideEffect.WriteDouble:
                FloatRegisterFile?.Doubles[(int)execution.FloatReg] = execution.DoubleWriteBack;
                break;
        }

        // Apply the program counter update
        ProgramCounter = nextPc;
        return RiscVTrap.None;
    }
}
