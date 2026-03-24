// Avishai Dernis 2026

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;

namespace Zarem.Emulator.Executor;

public partial struct InstructionServiceTable
{
    private MipsTrap CreateCoProc0Execution(MipsInstruction inst, out Execution exec)
    {
        // Check if the current privilege mode allows executing coprocessor instructions
        // NOTE: Make mfc0 permissions in user mode configurable?
        if (Processor.CoProcessor0.PrivilegeMode is not PrivilegeMode.Kernel)
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
                Co0FuncCode.ExceptionReturn => Eret(),
                Co0FuncCode.ReadIndexedTLBEntry => Execution.CreateEffect(SideEffect.TLBRead),
                Co0FuncCode.WriteIndexedTLBEntry => Execution.CreateEffect(SideEffect.TLBWriteIndexed),
                Co0FuncCode.WriteRandomTLBEntry => Execution.CreateEffect(SideEffect.TLBWriteRandom),
                Co0FuncCode.ProbeTLBForMatch => Execution.CreateEffect(SideEffect.TLBProbe),

                _ => throw new NotImplementedException()
            },

            // MFMC0 Instructions
            CoProc0RSCode.MFMC0 => coInst.MFMC0FuncCode switch
            {
                MFMC0FuncCode.EnableInterrupts => SetInterrupts(inst, true),
                MFMC0FuncCode.DisableInterrupts => SetInterrupts(inst, false),

                _ => throw new NotImplementedException()
            },

            // Move instructions
            CoProc0RSCode.MFC0 => Execution.CreateWriteback(inst.RT, Processor.CoProcessor0[(CP0Registers)inst.RD]),
            CoProc0RSCode.MTC0 => Execution.CreateWriteback((CP0Registers)inst.RD, Processor[inst.RT]),

            _ => throw new NotImplementedException()
        };

        return MipsTrap.None;
    }

    private Execution Eret()
    {
        // Retrieve the status register value
        var status = Processor.CoProcessor0.StatusRegister;

        // Determine the target program counter based on the error level
        uint targetPC = status.ErrorLevel
            ? Processor.CoProcessor0[CP0Registers.ErrorEPC]
            : Processor.CoProcessor0[CP0Registers.ExceptionPC];

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
        return new Execution
        {
            CoProc0Reg = CP0Registers.Status,
            CoProcWriteBack = (uint)status,
            ProgramCounter = targetPC,
        };
    }

    private Execution SetInterrupts(CoProc0Instruction inst, bool enabled)
    {
        // Retrieve the status register
        var status = Processor.CoProcessor0.StatusRegister;

        // Apply the update function
        status.InteruptEnabled = enabled;

        if (inst.RT is not GPRegister.Zero)
        {
            // Write the updated status register value back to the specified GPR
            return new Execution
            {
                CoProc0Reg = CP0Registers.Status,
                CoProcWriteBack = (uint)status,
                WriteBack = (uint)status,
                GPR = inst.RT,
            };
        }

        return Execution.CreateWriteback(CP0Registers.Status, (uint)status);
    }
}
