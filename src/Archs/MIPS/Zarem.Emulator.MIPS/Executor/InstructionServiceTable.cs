// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Machine;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Executor;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public readonly partial struct InstructionServiceTable
{
    private readonly ExecutionDelegate[] _opCodeTable = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _specialTable = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _special2Table = new ExecutionDelegate[64];
    private readonly ExecutionDelegate[] _regImmTable = new ExecutionDelegate[32];

    // Execution delegate
    delegate MipsTrap ExecutionDelegate(MipsInstruction inst, out Execution execution);

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
    public MipsTrap Execute(MipsInstruction instruction, out Execution execution)
    {
        var func = _opCodeTable[(int)instruction.OpCode] ?? throw new NotImplementedException();
        return func(instruction, out execution);
    }

    private MipsTrap DispatchSpecial(MipsInstruction inst, out Execution exec)
    {
        var func = _specialTable[(int)inst.FuncCode] ?? throw new NotImplementedException();
        return func(inst, out exec);
    }

    private MipsTrap DispatchSpecial2(MipsInstruction inst, out Execution exec)
    {
        var func = _special2Table[(int)inst.FuncCode] ?? throw new NotImplementedException();
        return func(inst, out exec);
    }

    private MipsTrap DispatchRegImm(MipsInstruction inst, out Execution exec)
    {
        var func = _regImmTable[(int)inst.FuncCode] ?? throw new NotImplementedException();
        return func(inst, out exec);
    }

    private MipsTrap Shift<T>(MipsInstruction inst, out Execution exec)
        where T : IShiftLogic
    {
        exec = Execution.CreateWriteback(inst.RD, T.Execute(Processor[inst.RT], inst.ShiftAmount));
        return MipsTrap.None;
    }

    private MipsTrap ShiftVar<T>(MipsInstruction inst, out Execution exec)
        where T : IShiftLogic
    {
        exec = Execution.CreateWriteback(inst.RD, T.Execute(Processor[inst.RT], (int)Processor[inst.RS]));
        return MipsTrap.None;
    }

    private MipsTrap AluR<T>(MipsInstruction inst, out Execution exec)
        where T : IAluLogic
    {
        exec = Execution.CreateWriteback(inst.RD, T.Compute(Processor[inst.RS], Processor[inst.RT]));
        return MipsTrap.None;
    }

    private MipsTrap CheckedAluR<T>(MipsInstruction inst, out Execution exec)
        where T : ICheckedAluLogic
    {
        var rs = Processor[inst.RS];
        var rt = Processor[inst.RT];
        var value = T.Compute(rs, rt);

        if (T.Overflow((int)rs, (int)rt, (int)value))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        exec = Execution.CreateWriteback(inst.RD, value);
        return MipsTrap.None;
    }

    private MipsTrap AluI<T>(MipsInstruction inst, out Execution exec)
        where T : IAluLogic
    {
        exec = Execution.CreateWriteback(inst.RT, T.Compute(Processor[inst.RS], (uint)(int)inst.ImmediateValue));
        return MipsTrap.None;
    }

    private MipsTrap CheckedAluI<T>(MipsInstruction inst, out Execution exec)
        where T : ICheckedAluLogic
    {
        var rs = Processor[inst.RS];
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

    private MipsTrap MultR<T>(MipsInstruction inst, out Execution exec)
        where T : IMultLogic
    {
        var rs = Processor[inst.RS];
        var rt = Processor[inst.RT];
        exec = Execution.CreateHighLow(T.Compute(rs, rt));
        return MipsTrap.None;
    }

    private MipsTrap MultAddR<T>(MipsInstruction inst, out Execution exec)
        where T : IMultAddLogic
    {
        var rs = Processor[inst.RS];
        var rt = Processor[inst.RT];
        exec = Execution.CreateHighLow(T.Compute(rs, rt, Processor.High, Processor.Low));
        return MipsTrap.None;
    }

    private MipsTrap DivR<T>(MipsInstruction inst, out Execution exec)
        where T : IDivLogic
    {
        var rs = Processor[inst.RS];
        var rt = Processor[inst.RT];
        exec = Execution.CreateHighLow((T.Remainder(rs, rs), T.Divisor(rs, rt)));
        return MipsTrap.None;
    }

    private static MipsTrap Trap<T>(MipsInstruction inst, out Execution exec)
        where T : ITrapLogic
    {
        exec = default;
        return T.Trap();
    }

    private MipsTrap BranchOn<T>(MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        if (T.Check(Processor[inst.RS], Processor[inst.RT]))
        {
            exec = Execution.CreateJump((uint)(Processor.ProgramCounter + inst.Offset + 4));
        }
        else
        {
            exec = default;
        }

        return MipsTrap.None;
    }

    private MipsTrap TrapOn<T>(MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        exec = default;
        return T.Check(Processor[inst.RS], Processor[inst.RT]) ? MipsTrap.Trap : MipsTrap.None;
    }

    private MipsTrap Load<T>(MipsInstruction inst, out Execution exec)
        where T : IBinaryInteger<T>
    {
        uint baseAddr = Processor[inst.RS];
        int offset = inst.ImmediateValue; // already sign-extended
        uint addr = baseAddr + (uint)offset;

        // Alignment check (bytes are always aligned)
        int size = Unsafe.SizeOf<T>();
        if (size > 1 && (addr & (uint)(size - 1)) != 0)
        {
            exec = default;
            return MipsTrap.AddressErrorStore;
        }

        bool signed = (-T.MultiplicativeIdentity) < T.Zero;
        exec = Execution.CreateMemRead(inst.RT, addr, size, signed);
        return MipsTrap.None;
    }

    private MipsTrap Store<T>(MipsInstruction inst, out Execution exec)
    {
        uint baseAddr = Processor[inst.RS];
        int offset = inst.ImmediateValue; // already sign-extended
        uint addr = baseAddr + (uint)offset;

        // Alignment check (bytes are always aligned)
        int size = Unsafe.SizeOf<T>();
        if (size > 1 && (addr & (uint)(size - 1)) != 0)
        {
            exec = default;
            return MipsTrap.AddressErrorStore;
        }

        exec = Execution.CreateMemWrite(Processor[inst.RT], addr, size);
        return MipsTrap.None;
    }

    private static MipsTrap Jump(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJump(inst.Address);
        return MipsTrap.None;
    }

    private MipsTrap JumpLink(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJumpAndLink(inst.Address, Processor.ProgramCounter + 4, inst.RD);
        return MipsTrap.None;
    }

    private MipsTrap JumpR(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJump(Processor[inst.RS]);
        return MipsTrap.None;
    }

    private MipsTrap JumpLinkR(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJumpAndLink(Processor[inst.RS], Processor.ProgramCounter + 4, inst.RD);
        return MipsTrap.None;
    }

    private MipsTrap Mfhi(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateWriteback(inst.RD, Processor.High);
        return MipsTrap.None;
    }

    private MipsTrap Mthi(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateHigh(Processor[inst.RS]);
        return MipsTrap.None;
    }

    private MipsTrap Mflo(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateWriteback(inst.RD, Processor.Low);
        return MipsTrap.None;
    }

    private MipsTrap Mtlo(MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateLow(Processor[inst.RS]);
        return MipsTrap.None;
    }
}
