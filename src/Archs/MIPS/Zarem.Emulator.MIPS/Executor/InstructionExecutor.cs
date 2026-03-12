// Avishai Dernis 2026

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Registers;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A class which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial class InstructionExecutor
{
    // R-Type delegates
    delegate uint BasicRDelegate(uint rs, uint rt);
    delegate ulong MultRDelegate(uint rs, uint rt);
    delegate uint ShiftRDelegate(uint rs, byte shift);

    // I-Type delegates
    delegate uint BasicIDelegate(uint rs, short imm);

    // CoProc0 delegates
    delegate void StatusUpdateDelegate(ref StatusRegister rs);
    
    // Misc delegates
    delegate bool BranchDelegate(uint rs, uint rt);
    delegate bool TrapIDelegate(uint rs, short rt);
    delegate bool OverflowCheckDelegate(int a, int b, int r);

    private MipsInstruction Instruction { get; }

    private MipsCpu Processor { get; }

    private MIPSTrap Trap { get; set; }

    private uint RS => Processor[Instruction.RS];

    private uint RT => Processor[Instruction.RT];

    private CoProc0Instruction CoProc0Instruction => (CoProc0Instruction)Instruction;

    private FloatInstruction FloatInstruction => (FloatInstruction)Instruction;

    private MIPSEmulatorConfig Config => Processor.Computer.Config;

    private InstructionExecutor(MipsInstruction instruction, MipsCpu processor)
    {
        Instruction = instruction;
        Processor = processor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="processor"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public static MIPSTrap Execute(MipsInstruction instruction, MipsCpu processor, out Execution execution)
    {
        var context = new InstructionExecutor(instruction, processor);
        execution = context.CreateExecution();
        return context.Trap;
    }

    private Execution CreateExecution()
    {
        return Instruction.OpCode switch
        {
            // Special (R-Type)
            OperationCode.Special or
            OperationCode.Special2 or
            OperationCode.Special3 => CreateRTypeExecution(),

            // Jump (J-Type)
            OperationCode.Jump => Execution.CreateJump(Instruction.Address),
            OperationCode.JumpAndLink => Execution.CreateJumpAndLink(Instruction.Address, Processor.ProgramCounter + 4),
            OperationCode.JumpAndLinkX => throw new NotImplementedException(),

            // Branch/Trap type (B-Type)
            OperationCode.RegisterImmediate => CreateRegImmExecution(),

            // Coprocessor instructions
            OperationCode.Coprocessor0 => CreateCo0Execution(),
            OperationCode.Coprocessor1 => CreateCoproc1Execution(),
            OperationCode.Coprocessor2 => throw new NotImplementedException(),
            OperationCode.Coprocessor3 => throw new NotImplementedException(),

            OperationCode.Trap => CreateTrap(MIPSTrap.Trap),
            OperationCode.SIMD => throw new NotImplementedException(),

            >= OperationCode.LoadByte and <= OperationCode.StoreWordRight => CreateMemoryExecution(),

            OperationCode.LoadLinkedWord => throw new NotImplementedException(),
            OperationCode.LoadWordCoprocessor1 => throw new NotImplementedException(),
            OperationCode.LoadWordCoprocessor2 => throw new NotImplementedException(),
            OperationCode.LoadWordCoprocessor3 => throw new NotImplementedException(),
            OperationCode.LoadDoubleWordCoprocessor1 => throw new NotImplementedException(),
            OperationCode.LoadDoubleWordCoprocessor2 => throw new NotImplementedException(),
            OperationCode.LoadDoubleWordCoprocessor3 => throw new NotImplementedException(),
            OperationCode.StoreConditionalWord => throw new NotImplementedException(),
            OperationCode.StoreWordCoprocessor1 => throw new NotImplementedException(),
            OperationCode.StoreWordCoprocessor2 => throw new NotImplementedException(),
            OperationCode.StoreWordCoprocessor3 => throw new NotImplementedException(),

            // Fall-through to I-Type by default. Most instructions are I-Type and they take up many opcodes.
            _ => CreateITypeExecution(),
        };
    }

    private Execution CreateTrap(MIPSTrap trap)
    {
        Trap = trap;
        return default;
    }
}
