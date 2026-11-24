// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Config;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Models;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.Machine;

/// <summary>
/// A base class representing a processor unit.
/// </summary>
public partial class MipsCpu<T>
{
    /// <summary>
    /// A <see cref="CpuBase{T}.IExecutor{TInstruction,TTrap}"/> for an interpreted MIPS cpu.
    /// </summary>
    protected class MipsInterpretedExecutor : IExecutor<MipsInstruction, MipsTrap>
    {
        private readonly IMipsInstructionServiceTable<T> _instructionServiceTable;
        private readonly MipsCpu<T> _cpu;

        /// <summary>
        /// Initializes a new instance of the <see cref="MipsInterpretedExecutor"/> class.
        /// </summary>
        /// <param name="cpu"></param>
        public MipsInterpretedExecutor(MipsCpu<T> cpu)
        {
            _cpu = cpu;

            _instructionServiceTable = _cpu.Config.VersionInfo.Is64Bit
                ? new MipsInstructionServiceTable<T, long>(_cpu)
                : new MipsInstructionServiceTable<T, int>(_cpu);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ExecutionLoop()
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

            if ((_cpu.ProgramCounter & T.CreateTruncating(0b11)) != T.Zero)
                return MipsTrap.AddressErrorLoad;

            instruction = (MipsInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(_cpu.ProgramCounter));
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
                _cpu.HandleTrap(trap);
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
                        accessResult = _cpu.Memory.TryRead(addr, out byte b);
                        read = signed ? T.CreateTruncating((sbyte)b) : T.CreateTruncating(b);
                        break;
                    case 2:
                        accessResult = _cpu.Memory.TryRead(addr, out ushort s);
                        read = signed ? T.CreateTruncating((short)s) : T.CreateTruncating(s);
                        break;
                    case 4:
                        accessResult = _cpu.Memory.TryRead(addr, out uint i);
                        read = signed ? T.CreateTruncating((int)i) : T.CreateTruncating(i);
                        break;
                    case 8:
                        accessResult = _cpu.Memory.TryRead(addr, out ulong l);
                        read = signed ? T.CreateTruncating((long)l) : T.CreateTruncating(l);
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
                    1 => _cpu.Memory.TryWrite(addr, byte.CreateTruncating(execution.Writeback)),
                    2 => _cpu.Memory.TryWrite(addr, ushort.CreateTruncating(execution.Writeback)),
                    4 => _cpu.Memory.TryWrite(addr, uint.CreateTruncating(execution.Writeback)),
                    8 => _cpu.Memory.TryWrite(addr, ulong.CreateTruncating(execution.Writeback)),
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
            T nextPc = _cpu.DelaySlot ?? (_cpu.ProgramCounter + T.CreateTruncating(4));
            _cpu.DelaySlot = null;

            // Handle gpr writeback
            if (execution.SideEffect is not (MipsSideEffect.ReadMemory or MipsSideEffect.WriteMemory))
                _cpu.RegisterFile[(int)execution.WritebackGPRegister] = execution.Writeback;

            // Apply side effects
            switch (execution.SideEffect)
            {
                case MipsSideEffect.Low:
                    _cpu.RegisterFile.Low = execution.Low;
                    break;
                case MipsSideEffect.High:
                    _cpu.RegisterFile.High = execution.High;
                    break;
                case MipsSideEffect.HighLow:
                    (_cpu.RegisterFile.High, _cpu.RegisterFile.Low) = (execution.High, execution.Low);
                    break;
                case MipsSideEffect.ProgramCounter:
                case MipsSideEffect.ForceProgramCounter:
                    ApplyJump(execution.ProgramCounter, ref nextPc, execution.SideEffect is MipsSideEffect.ForceProgramCounter);
                    break;
                case MipsSideEffect.ReadMemory:
                case MipsSideEffect.ReadMemorySigned:
                    _cpu.RegisterFile[(int)execution.WritebackGPRegister] = memRead;
                    break;
                case MipsSideEffect.WriteCoProc0:
                    _cpu.ControlCoProcessor[execution.CoProc0Reg] = execution.CoProcWriteBack;
                    break;
                case MipsSideEffect.TLBProbe:
                    _cpu.ControlCoProcessor.WritebackTlbp();
                    break;
                case MipsSideEffect.TLBRead:
                    _cpu.ControlCoProcessor.WritebackTlbr();
                    break;
                case MipsSideEffect.TLBWriteIndexed:
                    _cpu.ControlCoProcessor.WritebackTlbwi();
                    break;
                case MipsSideEffect.TLBWriteRandom:
                    _cpu.ControlCoProcessor.WritebackTlbwr();
                    break;
            }

            // Apply the program counter update
            _cpu.ProgramCounter = nextPc;
            return MipsTrap.None;
        }

        private void ApplyJump(T targetPc, ref T nextPc, bool force)
        {
            if (force || _cpu.Config.DisableDelaySlots)
            {
                // Branch delays are disabled. Just change the PC
                nextPc = targetPc;
                return;
            }

            // Store the branch offset in the delay slot
            _cpu.DelaySlot = targetPc;
        }
    }
}
