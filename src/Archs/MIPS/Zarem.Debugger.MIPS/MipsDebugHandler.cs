// Avishai Dernis 2026

using Zarem.Debugger.Handlers;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Debugger.MIPS;

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
        var breakInstruction = MipsInstruction.Create(FunctionCode.Break, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero);
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
        var instruction = (MipsInstruction)computer.Memory.Read<uint>(pc);

        if (instruction.Type is InstructionType.BasicJ)
            return instruction.Address;

        // TODO: Handle branch delay slots properly
        // and check the emulator config to see if they're enabled

        return pc + InstructionSize;
    }

    /// <inheritdoc/>
    public ulong GetStepOverAddress(IComputer computer)
    {
        var pc = computer.Cpu.ProgramCounter;
        var instruction = (MipsInstruction)computer.Memory.Read<uint>(pc);

        // If jump and link 
        if (instruction.OpCode is OperationCode.JumpAndLink ||
            (instruction.OpCode is OperationCode.Special && instruction.FuncCode is FunctionCode.JumpAndLinkRegister))
            return pc + InstructionSize;

        return GetStepAddress(computer);
    }

    /// <inheritdoc/>
    public ulong GetStepOutAddress(IComputer computer)
    {
        // TODO: Abstracted register API 
        throw new NotImplementedException();
    }
}
