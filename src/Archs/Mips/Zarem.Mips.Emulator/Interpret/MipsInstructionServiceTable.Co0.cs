// Avishai Dernis 2026

using System;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models;

public partial class MipsInstructionServiceTable<T, TS>
{
    private static MipsTrap CreateCoProc0Execution(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        // Check if the current privilege mode allows executing coprocessor instructions
        // NOTE: Make mfc0 permissions in user mode configurable?
        if (cpu.CoProcessor0.PrivilegeMode is not PrivilegeMode.Kernel)
        {
            exec = default;
            return MipsTrap.ReservedInstruction;
        }

        var coInst = (CoProc0Instruction)inst;
        exec = coInst.CoProc0RSCode switch
        {
            // C0 Instructions
            CoProc0RSCode.C0 => coInst.Co0FuncCode switch
            {
                Co0FuncCode.ExceptionReturn => Eret(cpu),
                Co0FuncCode.ReadIndexedTLBEntry => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBRead),
                Co0FuncCode.WriteIndexedTLBEntry => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBWriteIndexed),
                Co0FuncCode.WriteRandomTLBEntry => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBWriteRandom),
                Co0FuncCode.ProbeTLBForMatch => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBProbe),

                _ => throw new NotImplementedException()
            },

            // MFMC0 Instructions
            CoProc0RSCode.MFMC0 => coInst.MFMC0FuncCode switch
            {
                MFMC0FuncCode.EnableInterrupts => SetInterrupts(cpu, inst, true),
                MFMC0FuncCode.DisableInterrupts => SetInterrupts(cpu, inst, false),

                _ => throw new NotImplementedException()
            },

            // Move instructions
            CoProc0RSCode.MFC0 => MipsExecution<T>.CreateWriteback(inst.RT, cpu.CoProcessor0[(CP0Registers)inst.RD]),
            CoProc0RSCode.MTC0 => MipsExecution<T>.CreateWriteback((CP0Registers)inst.RD, cpu[inst.RT]),

            _ => throw new NotImplementedException()
        };

        return MipsTrap.None;
    }

    private static MipsExecution<T> Eret(MipsInterpretCpu<T> cpu)
    {
        // Retrieve the status register value
        var status = cpu.CoProcessor0.RegisterFile.StatusRegister;

        // Determine the target program counter based on the error level
        T targetPC = status.ErrorLevel
            ? cpu.CoProcessor0[CP0Registers.ErrorEPC]
            : cpu.CoProcessor0[CP0Registers.ExceptionPC];

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
            CoProc0WriteBack = T.CreateTruncating((uint)status),
            ProgramCounter = targetPC,
        };
    }

    private static MipsExecution<T> SetInterrupts(MipsInterpretCpu<T> cpu, CoProc0Instruction inst, bool enabled)
    {
        // Retrieve the status register
        var status = cpu.CoProcessor0.RegisterFile.StatusRegister;

        // Apply the update function
        status.InteruptEnabled = enabled;

        if (inst.RT is not MipsGpRegister.Zero)
        {
            // Write the updated status register value back to the specified GPR
            return new MipsExecution<T>
            {
                CoProc0Reg = CP0Registers.Status,
                CoProc0WriteBack = T.CreateTruncating((uint)status),
                Writeback = T.CreateTruncating((uint)status),
                WritebackGPRegister = inst.RT,
            };
        }

        return MipsExecution<T>.CreateWriteback(CP0Registers.Status, T.CreateTruncating((uint)status));
    }
}
