// Avishai Dernis 2026

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using System.ComponentModel;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public partial struct InstructionServiceTable
{
    private readonly ExecutionDelegate[] _opCodeTable = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _specialTable = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _special2Table = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _special3Table = new ExecutionDelegate[64];
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

        Initialize(processor.Config);
    }

    private MipsCpu Processor { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="processor"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public readonly MipsTrap Execute(MipsInstruction instruction, out Execution execution) => _opCodeTable[(int)instruction.OpCode](this, instruction, out execution);

    private readonly void Initialize(MIPSEmulatorConfig config)
    {
        InitSpecial(config);

        _opCodeTable[(int)OperationCode.Special] = DispatchSpecial;
    }

    private readonly void InitSpecial(MIPSEmulatorConfig config)
    {
        // Shift
        _specialTable[(int)FunctionCode.ShiftLeftLogical] = Shift<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogical] = Shift<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmetic] = Shift<SraLogic>;
        _specialTable[(int)FunctionCode.ShiftLeftLogicalVariable] = ShiftVar<SllLogic>;
        _specialTable[(int)FunctionCode.ShiftRightLogicalVariable] = ShiftVar<SrlLogic>;
        _specialTable[(int)FunctionCode.ShiftRightArithmeticVariable] = ShiftVar<SraLogic>;

        // Arithmetic
        _specialTable[(int)FunctionCode.Add] = CheckedAluR<AddLogic>;
        _specialTable[(int)FunctionCode.AddUnsigned] = AluR<AdduLogic>;
        _specialTable[(int)FunctionCode.Subtract] = CheckedAluR<SubLogic>;
        _specialTable[(int)FunctionCode.SubtractUnsigned] = AluR<SubuLogic>;
        _specialTable[(int)FunctionCode.Multiply] = MultR<MultLogic>;
        _specialTable[(int)FunctionCode.MultiplyUnsigned] = MultR<MultuLogic>;
        _specialTable[(int)FunctionCode.Divide] = DivR<DivLogic>;
        _specialTable[(int)FunctionCode.DivideUnsigned] = DivR<DivuLogic>;

        // Logical
        _specialTable[(int)FunctionCode.And] = AluR<AndLogic>;
        _specialTable[(int)FunctionCode.Or] = AluR<OrLogic>;
        _specialTable[(int)FunctionCode.ExclusiveOr] = AluR<XorLogic>;
        _specialTable[(int)FunctionCode.Nor] = AluR<NorLogic>;

        // Compare
        _specialTable[(int)FunctionCode.SetLessThan] = AluR<SltLogic>;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = AluR<SltuLogic>;

        // Jump Register
        _specialTable[(int)FunctionCode.SetLessThan] = JumpR;
        _specialTable[(int)FunctionCode.SetLessThanUnsigned] = JumpLinkR;

        // System
        _specialTable[(int)FunctionCode.SystemCall] = Trap<SyscallLogic>;
        _specialTable[(int)FunctionCode.Break] = Trap<BreakLogic>;
        _specialTable[(int)FunctionCode.Sync] = NotImplemented;
    }

    private static MipsTrap DispatchSpecial(InstructionServiceTable context, MipsInstruction inst, out Execution exec) => context._specialTable[(int)inst.FuncCode](context, inst, out exec);

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

    private static MipsTrap MultR<T>(InstructionServiceTable context, MipsInstruction inst, out Execution exec)
        where T : IMultLogic
    {
        exec = Execution.CreateHighLow(T.Compute(context.Processor[inst.RS], context.Processor[inst.RT]));
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

    }

    private static MipsTrap NotImplemented(InstructionServiceTable context, MipsInstruction inst, out Execution exec) => throw new NotImplementedException();
}
