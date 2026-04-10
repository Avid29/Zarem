// Avishai Dernis 2026

using System;
using Zarem.Debugger.Handlers;
using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Debugger;

/// <summary>
/// A <see cref="IDebugHandler"/> for the mips architecture.
/// </summary>
public class MipsDebugHandler : IDebugHandler
{
    private readonly byte[] _breakPointBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsDebugHandler"/> class.
    /// </summary>
    public MipsDebugHandler()
    {
        var breakInstruction = MipsInstruction.CreateR(FunctionCode.Break, MipsGpRegister.Zero, MipsGpRegister.Zero, MipsGpRegister.Zero);
        _breakPointBytes = BitConverter.GetBytes((uint)breakInstruction);

        if (BitConverter.IsLittleEndian)
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

        // If delay slots are enabled, this is easy. The CPU literally tracks where it will jump next
        var mipsCpu = (IMipsCpu)computer.Cpu;
        if (mipsCpu.DelaySlot.HasValue)
        {
            return mipsCpu.DelaySlot.Value;
        }
        else
        {
            var instruction = (MipsInstruction)computer.Memory.Read<uint>(pc);

            return instruction.OpCode switch
            {
                // Jumps
                MipsOpCode.Jump or MipsOpCode.JumpAndLink or MipsOpCode.JumpAndLinkX => instruction.Address,
                MipsOpCode.Special when instruction.FuncCode is FunctionCode.JumpRegister or FunctionCode.JumpAndLinkRegister => mipsCpu[instruction.RS],

                // Branches
                MipsOpCode.BranchCompact or MipsOpCode.BranchAndLinkCompact or
                (>= MipsOpCode.BranchOnEquals and <= MipsOpCode.BranchOnGreaterThanZero) or
                (>= MipsOpCode.BranchOnEqualLikely and <= MipsOpCode.BranchOnGreaterThanZeroLikely) => StepBranch(instruction, mipsCpu, false),

                // RT Branches
                MipsOpCode.RegisterImmediate when instruction.RTFuncCode is
                (>= RegImmFuncCode.BranchOnLessThanZero and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely) or
                (>= RegImmFuncCode.BranchOnLessThanZeroAndLink and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink) => StepBranch(instruction, mipsCpu, false),

                _ => pc + InstructionSize,
            };
        }
    }

    /// <inheritdoc/>
    public ulong GetStepOverAddress(IComputer computer)
    {
        var pc = computer.Cpu.ProgramCounter;
        var instruction = (MipsInstruction)computer.Memory.Read<uint>(pc);

        ulong skipAddress = pc + (InstructionSize * 2);

        // Skip over "and link" instructions and thier functions
        return instruction.OpCode switch
        {
            // Jump and link
            MipsOpCode.JumpAndLink => skipAddress,
            MipsOpCode.Special when instruction.FuncCode is FunctionCode.JumpAndLinkRegister => skipAddress,

            // Branch and link
            MipsOpCode.RegisterImmediate when instruction.RTFuncCode is > RegImmFuncCode.BranchOnLessThanZeroAndLink and < RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink => skipAddress,

            // Default
            _ => GetStepAddress(computer),
        };
    }

    /// <inheritdoc/>
    public ulong GetStepOutAddress(IComputer computer)
    {
        var mipsCpu = (IMipsCpu)computer.Cpu;
        return mipsCpu[MipsGpRegister.ReturnAddress];
    }

    /// <inheritdoc/>
    public IDebugViewer? GetDebugViewer(IComputer computer) => MipsDebugViewer.Create(computer);

    private ulong StepBranch(MipsInstruction instruction, IMipsCpu cpu, bool delayed)
    {
        var rs = cpu[instruction.RS];
        var rt = cpu[instruction.RT];

        bool branch = instruction.OpCode switch
        {
            MipsOpCode.BranchOnEquals or MipsOpCode.BranchOnEqualLikely => rs == rt,
            MipsOpCode.BranchOnNotEquals or MipsOpCode.BranchOnNotEqualLikely => rs != rt,
            MipsOpCode.BranchOnLessThanOrEqualToZero or MipsOpCode.BranchOnLessThanOrEqualToZeroLikely => (int)rs <= 0,
            MipsOpCode.BranchOnGreaterThanZero or MipsOpCode.BranchOnGreaterThanZeroLikely => (int)rs > 0,
            MipsOpCode.RegisterImmediate => instruction.RTFuncCode switch
            {
                RegImmFuncCode.BranchOnLessThanZero or
                RegImmFuncCode.BranchOnLessThanZeroLikely or
                RegImmFuncCode.BranchOnLessThanZeroAndLink or
                RegImmFuncCode.BranchOnLessThanZeroLikelyAndLink => (int)rs < 0,

                RegImmFuncCode.BranchOnGreaterThanOrEqualToZero or
                RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely or
                RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroAndLink or
                RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink => (int)rs >= 0,

                _ => false
            },

            MipsOpCode.BranchCompact or MipsOpCode.BranchAndLinkCompact => true,
            _ => false,
        };

        // MIPS branch targets are calculated from the instruction AFTER the branch (the delay slot)
        // Target = (PC + 4) + offset
        if (branch)
        {
            return cpu.ProgramCounter + (delayed ? InstructionSize : 0) + (ulong)instruction.Offset;
        }

        // If not taken, we move to the instruction after the delay slot
        return cpu.ProgramCounter + (delayed ? (InstructionSize * 2) : InstructionSize);
    }
}
