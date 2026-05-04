// Avishai Dernis 2026

using System;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models;

public partial class MipsInstructionServiceTable<T, TS>
{
    private static MipsTrap CreateCoProc0Execution(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        // Check if the current privilege mode allows executing coprocessor instructions
        // NOTE: Make mfc0 permissions in user mode configurable?
        if (@this._cpu.CoProcessor0.PrivilegeMode is not PrivilegeMode.Kernel)
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
                Co0FuncCode.ExceptionReturn => @this.Eret(),
                Co0FuncCode.ReadIndexedTLBEntry => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBRead),
                Co0FuncCode.WriteIndexedTLBEntry => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBWriteIndexed),
                Co0FuncCode.WriteRandomTLBEntry => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBWriteRandom),
                Co0FuncCode.ProbeTLBForMatch => MipsExecution<T>.CreateEffect(MipsSideEffect.TLBProbe),

                _ => throw new NotImplementedException()
            },

            // MFMC0 Instructions
            CoProc0RSCode.MFMC0 => coInst.MFMC0FuncCode switch
            {
                MFMC0FuncCode.EnableInterrupts => @this.SetInterrupts(inst, true),
                MFMC0FuncCode.DisableInterrupts => @this.SetInterrupts(inst, false),

                _ => throw new NotImplementedException()
            },

            // Move instructions
            CoProc0RSCode.MFC0 => MipsExecution<T>.CreateWriteback(inst.RT, @this._cpu.CoProcessor0[(CP0Registers)inst.RD]),
            CoProc0RSCode.MTC0 => MipsExecution<T>.CreateWriteback((CP0Registers)inst.RD, @this._cpu[inst.RT]),

            _ => throw new NotImplementedException()
        };

        return MipsTrap.None;
    }

    private MipsExecution<T> Eret()
    {
        // Retrieve the status register value
        var status = _cpu.CoProcessor0.StatusRegister;

        // Determine the target program counter based on the error level
        T targetPC = status.ErrorLevel
            ? _cpu.CoProcessor0[CP0Registers.ErrorEPC]
            : _cpu.CoProcessor0[CP0Registers.ExceptionPC];

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

    private MipsExecution<T> SetInterrupts(CoProc0Instruction inst, bool enabled)
    {
        // Retrieve the status register
        var status = _cpu.CoProcessor0.StatusRegister;

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
