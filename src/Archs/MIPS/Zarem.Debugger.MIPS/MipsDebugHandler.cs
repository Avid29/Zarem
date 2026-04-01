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
        var breakInstruction = MipsInstruction.CreateR(FunctionCode.Break, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero);
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
        var mipsCpu = (MipsCpu)computer.Cpu;
        if (mipsCpu.DelaySlot.HasValue)
        {
            return mipsCpu.DelaySlot.Value;
        }

        // TODO: Handle jumps when the delay slot is disabled

        return pc + InstructionSize;
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
            OperationCode.JumpAndLink => skipAddress,
            OperationCode.Special when instruction.FuncCode is FunctionCode.JumpAndLinkRegister => skipAddress,

            // Branch and link
            OperationCode.RegisterImmediate when instruction.RTFuncCode is > RegImmFuncCode.BranchOnLessThanZeroAndLink and < RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink => skipAddress,

            // Default
            _ => GetStepAddress(computer),
        };
    }

    /// <inheritdoc/>
    public ulong GetStepOutAddress(IComputer computer)
    {
        var mipsCpu = (MipsCpu)computer.Cpu;
        return mipsCpu[(int)GPRegister.ReturnAddress];
    }

    /// <inheritdoc/>
    public IDebugViewer? GetDebugViewer(IComputer computer) => MipsDebugViewer.Create(computer);
}
