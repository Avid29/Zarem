// Avishai Dernis 2026

using System;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial class InstructionExecutor
{
    private Execution CreateCo0Execution()
    {
        // Check if the current privilege mode allows executing coprocessor instructions
        // NOTE: Make mfc0 permissions in user mode configurable?
        if (Processor.CoProcessor0.PrivilegeMode is not PrivilegeMode.Kernel)
            return CreateTrap(MIPSTrap.ReservedInstruction);

        return CoProc0Instruction.CoProc0RSCode switch
        {
            // C0 Instructions
            CoProc0RSCode.C0 => CoProc0Instruction.Co0FuncCode switch
            {
                Co0FuncCode.ExceptionReturn => Eret(),
                Co0FuncCode.ReadIndexedTLBEntry => Execution.CreateEffect(SideEffect.TLBRead),
                Co0FuncCode.WriteIndexedTLBEntry => Execution.CreateEffect(SideEffect.TLBWriteIndexed),
                Co0FuncCode.WriteRandomTLBEntry => Execution.CreateEffect(SideEffect.TLBWriteRandom),
                Co0FuncCode.ProbeTLBForMatch => Execution.CreateEffect(SideEffect.TLBProbe),

                _ => throw new NotImplementedException()
            },

            // MFMC0 Instructions
            CoProc0RSCode.MFMC0 => CoProc0Instruction.MFMC0FuncCode switch
            {
                MFMC0FuncCode.EnableInterrupts => StatusUpdateInstruction((ref status) => status.InteruptEnabled = true),
                MFMC0FuncCode.DisableInterrupts => StatusUpdateInstruction((ref status) => status.InteruptEnabled = false),

                _ => throw new NotImplementedException()
            },

            // Move instructions
            CoProc0RSCode.MFC0 => Execution.CreateWriteback(Instruction.RT, Processor.CoProcessor0[(CP0Registers)Instruction.RD]),
            CoProc0RSCode.MTC0 => Execution.CreateWriteback((CP0Registers)Instruction.RD, RT),

            _ => throw new NotImplementedException()
        };
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

    private Execution StatusUpdateInstruction(StatusUpdateDelegate func)
    {
        // Retrieve the status register
        var status = Processor.CoProcessor0.StatusRegister;

        // Apply the update function
        func(ref status);

        if (Instruction.RT is not GPRegister.Zero)
        {
            // Write the updated status register value back to the specified GPR
            return new Execution
            {
                CoProc0Reg = CP0Registers.Status,
                CoProcWriteBack = (uint)status,
                WriteBack = (uint)status,
                GPR = Instruction.RT,
            };
        }

        return Execution.CreateWriteback(CP0Registers.Status, (uint)status);
    }
}
