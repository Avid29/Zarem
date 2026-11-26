// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.CoProcessors.Control;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors.Control;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="MipsControlExecution{T}"/> models.
/// </summary>
public partial class MipsControlInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly MipsCpu<T> _cpu;
    private readonly ControlCoProcessor<T> _c0;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsControlInstructionServiceTable{T}"/> struct.
    /// </summary>
    public MipsControlInstructionServiceTable(MipsCpu<T> cpu, ControlCoProcessor<T> c0)
    {
        _cpu = cpu;
        _c0 = c0;
    }

    /// <inheritdoc/>
    public MipsTrap Execute(MipsInstruction instruction, out MipsControlExecution<T> execution)
    {
        // Check if the current privilege mode allows executing coprocessor instructions
        // NOTE: Make mfc0 permissions in user mode configurable?
        if (_c0.ActingPrivilegeMode is not PrivilegeMode.Kernel)
        {
            execution = default;
            return MipsTrap.ReservedInstruction;
        }

        var coInst = (CoProc0Instruction)instruction;
        execution = coInst.CoProc0RSCode switch
        {
            // C0 Instructions
            CoProc0RSCode.C0 => coInst.Co0FuncCode switch
            {
                Co0FuncCode.ExceptionReturn => Eret(),
                Co0FuncCode.ReadIndexedTLBEntry => MipsControlExecution<T>.CreateTlbOperation(TlbOperation.TLBRead),
                Co0FuncCode.WriteIndexedTLBEntry => MipsControlExecution<T>.CreateTlbOperation(TlbOperation.TLBWriteIndexed),
                Co0FuncCode.WriteRandomTLBEntry => MipsControlExecution<T>.CreateTlbOperation(TlbOperation.TLBWriteRandom),
                Co0FuncCode.ProbeTLBForMatch => MipsControlExecution<T>.CreateTlbOperation(TlbOperation.TLBProbe),

                _ => throw new NotImplementedException()
            },

            // MFMC0 Instructions
            CoProc0RSCode.MFMC0 => coInst.MFMC0FuncCode switch
            {
                MFMC0FuncCode.EnableInterrupts => SetInterrupts(instruction, true),
                MFMC0FuncCode.DisableInterrupts => SetInterrupts(instruction, false),

                _ => throw new NotImplementedException()
            },

            // Move instructions
            CoProc0RSCode.MFC0 => MipsExecution<T>.CreateWriteback(instruction.RT, _c0[(CP0Registers)instruction.RD]),
            CoProc0RSCode.MTC0 => MipsExecution<T>.CreateWriteback((CP0Registers)instruction.RD, _cpu[instruction.RT]),

            _ => throw new NotImplementedException()
        };

        return MipsTrap.None;
    }

    private MipsExecution<T> Eret()
    {
        // Retrieve the status register value
        var status = _c0.RegisterFile.StatusRegister;

        // Determine the target program counter based on the error level
        T targetPC = status.ErrorLevel
            ? _c0[CP0Registers.ErrorEPC]
            : _c0[CP0Registers.ExceptionPC];

        // Clear the appropriate level bit in the status register
        if (status.ErrorLevel)
        {
            status.ErrorLevel = false;
        }
        else
        {
            status.ExceptionLevel = false;
        }

        // TODO: Explorer special commit phase to avoid setting
        // the status register as a writeback
        return new MipsExecution<T>
        {
            CoProc0Reg = CP0Registers.Status,
            CoProcWriteBack = T.CreateTruncating((uint)status),
            ProgramCounter = targetPC,
        };
    }

    private MipsExecution<T> SetInterrupts(CoProc0Instruction inst, bool enabled)
    {
        // Retrieve the status register
        var status = _c0.RegisterFile.StatusRegister;

        // Apply the update function
        status.InteruptEnabled = enabled;

        if (inst.RT is not MipsGpRegister.Zero)
        {
            // Write the updated status register value back to the specified GPR
            return new MipsExecution<T>
            {
                CoProc0Reg = CP0Registers.Status,
                CoProcWriteBack = T.CreateTruncating((uint)status),
                Writeback = T.CreateTruncating((uint)status),
                WritebackGPRegister = inst.RT,
            };
        }

        return MipsExecution<T>.CreateWriteback(CP0Registers.Status, T.CreateTruncating((uint)status));
    }
}
