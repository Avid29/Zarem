// Avishai Dernis 2026

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Machine;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial struct InstructionServiceTable
{
    private readonly ExecutionDelegate[] _opCodeTable = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _specialTable = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _special2Table = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _regImmTable = new ExecutionDelegate[32];

    // Execution delegate
    delegate MipsTrap ExecutionDelegate(InstructionServiceTable context, MipsInstruction inst, out Execution execution);

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionServiceTable"/> struct.
    /// </summary>
    /// <param name="processor"></param>
    public InstructionServiceTable(MipsCpu processor)
    {
        Processor = processor;

        InitTables(processor.Config);
    }

    private MipsCpu Processor { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public readonly MipsTrap Execute(MipsInstruction instruction, out Execution execution)
    {
        var func = _opCodeTable[(int)instruction.OpCode] ?? throw new NotImplementedException();
        return func(this, instruction, out execution);
    }

    private static MipsTrap DispatchSpecial(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        var func = context._specialTable[(int)inst.FuncCode] ?? throw new NotImplementedException();
        return func(context, inst, out exec);
    }

    private static MipsTrap DispatchSpecial2(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        var func = context._special2Table[(int)inst.FuncCode] ?? throw new NotImplementedException();
        return func(context, inst, out exec);
    }

    private static MipsTrap DispatchRegImm(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        var func = context._regImmTable[(int)inst.FuncCode] ?? throw new NotImplementedException();
        return func(context, inst, out exec);
    }

    private static MipsTrap Shift<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IShiftLogic
    {
        exec = Execution.CreateWriteback(inst.RD, T.Execute(context.Processor[inst.RT], inst.ShiftAmount));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftVar<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IShiftLogic
    {
        exec = Execution.CreateWriteback(inst.RD, T.Execute(context.Processor[inst.RT], (int)context.Processor[inst.RS]));
        return MipsTrap.None;
    }

    private static MipsTrap AluR<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IAluLogic
    {
        exec = Execution.CreateWriteback(inst.RD, T.Compute(context.Processor[inst.RS], context.Processor[inst.RT]));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluR<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : ICheckedAluLogic
    {
        var rs = context.Processor[inst.RS];
        var rt = context.Processor[inst.RT];
        var value = T.Compute(rs, rt);

        if (T.Overflow((int)rs, (int)rt, (int)value))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        exec = Execution.CreateWriteback(inst.RD, value);
        return MipsTrap.None;
    }

    private static MipsTrap AluI<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IAluLogic
    {
        exec = Execution.CreateWriteback(inst.RT, T.Compute(context.Processor[inst.RS], (uint)(int)inst.ImmediateValue));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluI<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : ICheckedAluLogic
    {
        var rs = context.Processor[inst.RS];
        var imm = (uint)(int)inst.ImmediateValue;
        var value = T.Compute(rs, imm);

        if (T.Overflow((int)rs, (int)imm, (int)value))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        exec = Execution.CreateWriteback(inst.RT, value);
        return MipsTrap.None;
    }

    private static MipsTrap MultR<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IMultLogic
    {
        var rs = context.Processor[inst.RS];
        var rt = context.Processor[inst.RT];
        exec = Execution.CreateHighLow(T.Compute(rs, rt));
        return MipsTrap.None;
    }

    private static MipsTrap MultAddR<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IMultAddLogic
    {
        var rs = context.Processor[inst.RS];
        var rt = context.Processor[inst.RT];
        exec = Execution.CreateHighLow(T.Compute(rs, rt, context.Processor.High, context.Processor.Low));
        return MipsTrap.None;
    }

    private static MipsTrap DivR<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IDivLogic
    {
        var rs = context.Processor[inst.RS];
        var rt = context.Processor[inst.RT];
        exec = Execution.CreateHighLow((T.Remainder(rs, rs), T.Divisor(rs, rt)));
        return MipsTrap.None;
    }

    private static MipsTrap Trap<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : ITrapLogic
    {
        exec = default;
        return T.Trap();
    }

    private static MipsTrap BranchOn<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        if (T.Check(context.Processor[inst.RS], context.Processor[inst.RT]))
        {
            exec = Execution.CreateJump((uint)(context.Processor.ProgramCounter + inst.Offset + 4));
        }
        else
        {
            exec = default;
        }

        return MipsTrap.None;
    }

    private static MipsTrap TrapOn<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        exec = default;
        return T.Check(context.Processor[inst.RS], context.Processor[inst.RT]) ? MipsTrap.Trap : MipsTrap.None;
    }

    private static MipsTrap Jump(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJump(inst.Address);
        return MipsTrap.None;
    }

    private static MipsTrap JumpLink(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJumpAndLink(inst.Address, context.Processor.ProgramCounter + 4, inst.RD);
        return MipsTrap.None;
    }

    private static MipsTrap JumpR(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJump(context.Processor[inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap JumpLinkR(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJumpAndLink(context.Processor[inst.RS], context.Processor.ProgramCounter + 4, inst.RD);
        return MipsTrap.None;
    }

    private static MipsTrap Mfhi(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateWriteback(inst.RD, context.Processor.High);
        return MipsTrap.None;
    }

    private static MipsTrap Mthi(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateHigh(context.Processor[inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap Mflo(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateWriteback(inst.RD, context.Processor.Low);
        return MipsTrap.None;
    }

    private static MipsTrap Mtlo(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateLow(context.Processor[inst.RS]);
        return MipsTrap.None;
    }
}
