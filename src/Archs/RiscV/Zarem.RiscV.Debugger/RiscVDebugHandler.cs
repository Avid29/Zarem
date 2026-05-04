// Avishai Dernis 2026

using System;
using Zarem.Debugger.Handlers;
using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.RiscV.Debugger.Viewer;
using Zarem.RiscV.Emulator.Machine;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Debugger;

/// <summary>
/// A <see cref="IDebugHandler"/> for the mips architecture.
/// </summary>
public class RiscVDebugHandler : IDebugHandler
{
    private readonly byte[] _breakPointBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVDebugHandler"/> class.
    /// </summary>
    public RiscVDebugHandler()
    {
        var breakInstruction = RiscVInstruction.CreateI(RiscVOpCode.System, Funct3Code.EcallBreak, 0, 0, 1);
        _breakPointBytes = BitConverter.GetBytes((uint)breakInstruction);

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(_breakPointBytes);
        }
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> BreakpointBytes => _breakPointBytes;

    /// <inheritdoc/>
    public uint InstructionSize => 4;

    /// <inheritdoc/>
    public ulong GetStepAddress(IComputer computer)
    {
        var pc = computer.Cpu.ProgramCounter;

        var riscVCpu = (IRiscVCpu)computer.Cpu;
        var instruction = (RiscVInstruction)computer.Memory.Read<uint>(pc);

        return instruction.OpCode switch
        {
            RiscVOpCode.JumpAndLink => pc + (ulong)instruction.JumpOffset,
            RiscVOpCode.JumpAndLinkRegister => (riscVCpu[instruction.RS1] + (ulong)instruction.Immediate) & ~1UL,
            RiscVOpCode.Branch => StepBranch(instruction, riscVCpu),
            _ => pc + InstructionSize,
        };
    }

    /// <inheritdoc/>
    public ulong GetStepOverAddress(IComputer computer)
    {
        var pc = computer.Cpu.ProgramCounter;
        var instruction = (RiscVInstruction)computer.Memory.Read<uint>(pc);

        if (instruction.OpCode is RiscVOpCode.JumpAndLink or RiscVOpCode.JumpAndLinkRegister &&
            instruction.RD is not RiscVGpRegister.Zero)
        {
            return pc + InstructionSize;
        }

        return GetStepAddress(computer);
    }

    /// <inheritdoc/>
    public ulong GetStepOutAddress(IComputer computer)
    {
        var riscVCpu = (IRiscVCpu)computer.Cpu;
        return riscVCpu[RiscVGpRegister.ReturnAddress] & ~1UL;
    }

    /// <inheritdoc/>
    public IDebugViewer? GetDebugViewer(IComputer computer) => RiscVDebugViewer.Create(computer);

    private ulong StepBranch(RiscVInstruction instruction, IRiscVCpu cpu)
    {
        var nextPc = cpu.ProgramCounter + InstructionSize;
        var rs1 = cpu[instruction.RS1];
        var rs2 = cpu[instruction.RS2];

        bool branch = instruction.Funct3 switch
        {
            Funct3Code.BranchEqual => rs1 == rs2,
            Funct3Code.BranchNotEqual => rs1 != rs2,
            Funct3Code.BranchLessThan => (long)rs1 < (long)rs2,
            Funct3Code.BranchGreaterThanOrEqual => (long)rs1 >= (long)rs2,
            Funct3Code.BranchLessThanUnsigned => (long)rs1 < (long)rs2,
            Funct3Code.BranchGreaterThanOrEqualUnsigned => rs1 >= rs2,
            _ => false,
        };

        if (branch)
        {
            nextPc += (ulong)instruction.BranchOffset;
        }

        return nextPc;
    }
}
