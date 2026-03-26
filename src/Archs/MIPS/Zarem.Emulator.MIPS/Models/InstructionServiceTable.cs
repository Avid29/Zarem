// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Models.Enum;
using Zarem.Emulator.Machine;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="Execution"/> models.
/// </summary>
public unsafe partial class InstructionServiceTable
{
    private readonly delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[] _opCodeTable = new delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[64];
    private readonly delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[] _specialTable = new delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[64];
    private readonly delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[] _special2Table = new delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[64];
    private readonly delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[] _regImmTable = new delegate*<InstructionServiceTable, MipsInstruction, out Execution, MipsTrap>[32];
    private readonly MipsCpu _processor;
    private readonly uint* _regs;

    // Execution delegate
    delegate MipsTrap ExecutionDelegate(MipsInstruction inst, out Execution execution);

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionServiceTable"/> struct.
    /// </summary>
    /// <param name="processor"></param>
    public InstructionServiceTable(MipsCpu processor)
    {
        _processor = processor;
        _regs = processor.RegisterFile.Regs;

        InitTables(processor.Config);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public MipsTrap Execute(MipsInstruction instruction, out Execution execution)
    {
        var func = _opCodeTable[(int)instruction.OpCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(this, instruction, out execution);
    }

    private static MipsTrap DispatchSpecial(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        var func = @this._specialTable[(int)inst.FuncCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(@this, inst, out exec);
    }

    private static MipsTrap DispatchSpecial2(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        var func = @this._special2Table[(int)inst.FuncCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(@this, inst, out exec);
    }

    private static MipsTrap DispatchRegImm(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        var func = @this._regImmTable[(int)inst.RTFuncCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(@this, inst, out exec);
    }

    private static MipsTrap Shift<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IShiftLogic
    {
        var rt = @this._processor[inst.RT];
        exec = Execution.CreateWriteback(inst.RD, T.Execute(rt, inst.ShiftAmount));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftVar<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IShiftLogic
    {
        var rs = @this._regs[(int)inst.RS];
        var rt = @this._regs[(int)inst.RT];
        exec = Execution.CreateWriteback(inst.RD, T.Execute(rt, (int)rs));
        return MipsTrap.None;
    }

    private static MipsTrap AluR<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IAluLogic
    {
        var rs = @this._regs[(int)inst.RS];
        var rt = @this._regs[(int)inst.RT];
        exec = Execution.CreateWriteback(inst.RD, T.Compute(rs, rt));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluR<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : ICheckedAluLogic
    {
        var rs = @this._regs[(int)inst.RS];
        var rt = @this._regs[(int)inst.RT];
        var value = T.Compute(rs, rt);

        if (T.Overflow((int)rs, (int)rt, (int)value))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        exec = Execution.CreateWriteback(inst.RD, value);
        return MipsTrap.None;
    }

    private static MipsTrap AluI<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IAluLogic
    {
        exec = Execution.CreateWriteback(inst.RT, T.Compute(@this._regs[(int)inst.RS], (ushort)inst.ImmediateValue));
        return MipsTrap.None;
    }

    private static MipsTrap AluISigned<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IAluLogic
    {
        exec = Execution.CreateWriteback(inst.RT, T.Compute(@this._regs[(int)inst.RS], (uint)(int)inst.ImmediateValue));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluI<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : ICheckedAluLogic
    {
        var rs = @this._regs[(int)inst.RS];
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

    private static MipsTrap MultR<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IMultLogic
    {
        var rs = @this._processor[inst.RS];
        var rt = @this._processor[inst.RT];
        exec = Execution.CreateHighLow(T.Compute(rs, rt));
        return MipsTrap.None;
    }

    private static MipsTrap MultAddR<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IMultAddLogic
    {
        var rs = @this._processor[inst.RS];
        var rt = @this._processor[inst.RT];
        exec = Execution.CreateHighLow(T.Compute(rs, rt, @this._processor.High, @this._processor.Low));
        return MipsTrap.None;
    }

    private static MipsTrap DivR<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IDivLogic
    {
        var rs = @this._regs[(int)inst.RS];
        var rt = @this._regs[(int)inst.RT];
        exec = Execution.CreateHighLow((T.Remainder(rs, rt), T.Divisor(rs, rt)));
        return MipsTrap.None;
    }

    private static MipsTrap Trap<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : ITrapLogic
    {
        exec = default;
        return T.Trap();
    }

    private static MipsTrap BranchOn<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        var rs = @this._regs[(int)inst.RS];
        var rt = @this._regs[(int)inst.RT];
        exec = T.Check(rs, rt) ? Execution.CreateJump((uint)(@this._processor.ProgramCounter + inst.Offset + 4)) : default;
        return MipsTrap.None;
    }

    private static MipsTrap TrapOn<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        var rs = @this._regs[(int)inst.RS];
        var rt = @this._regs[(int)inst.RT];
        exec = default;
        return T.Check(rs, rt) ? MipsTrap.Trap : MipsTrap.None;
    }

    private static MipsTrap TrapOnI<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        exec = default;
        return T.Check(@this._regs[(int)inst.RS], (uint)(int)inst.ImmediateValue) ? MipsTrap.Trap : MipsTrap.None;
    }

    private static MipsTrap Move<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : ICondLogic
    {
        var rs = @this._regs[(int)inst.RS];
        var rt = @this._regs[(int)inst.RT];
        exec = T.Check(rs, rt) ? Execution.CreateWriteback(inst.RD, rs) : default;
        return MipsTrap.None;
    }

    private static MipsTrap Load<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
        where T : IBinaryInteger<T>
    {
        uint baseAddr = @this._regs[(int)inst.RS];
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

    private static MipsTrap Store<T>(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        uint baseAddr = @this._regs[(int)inst.RS];
        int offset = inst.ImmediateValue; // already sign-extended
        uint addr = baseAddr + (uint)offset;

        // Alignment check (bytes are always aligned)
        int size = Unsafe.SizeOf<T>();
        if (size > 1 && (addr & (uint)(size - 1)) != 0)
        {
            exec = default;
            return MipsTrap.AddressErrorStore;
        }

        exec = Execution.CreateMemWrite(@this._regs[(int)inst.RT], addr, size);
        return MipsTrap.None;
    }

    private static MipsTrap Jump(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJump(inst.Address);
        return MipsTrap.None;
    }

    private static MipsTrap JumpLink(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJumpAndLink(inst.Address, @this._processor.ProgramCounter + 4);
        return MipsTrap.None;
    }

    private static MipsTrap JumpR(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateJump(@this._regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap JumpLinkR(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        var rs = @this._regs[(int)inst.RS];
        exec = Execution.CreateJumpAndLink(rs, @this._processor.ProgramCounter + 4, inst.RD);
        return MipsTrap.None;
    }

    private static MipsTrap Mfhi(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateWriteback(inst.RD, @this._processor.High);
        return MipsTrap.None;
    }

    private static MipsTrap Mthi(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateHigh(@this._regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap Mflo(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateWriteback(inst.RD, @this._processor.Low);
        return MipsTrap.None;
    }

    private static MipsTrap Mtlo(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateLow(@this._regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap Lui(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = Execution.CreateWriteback(inst.RT, (uint)(ushort)inst.ImmediateValue << 16);
        return MipsTrap.None;
    }

    private static MipsTrap ReservedInstruction(InstructionServiceTable @this, MipsInstruction inst, out Execution exec)
    {
        exec = default;
        return MipsTrap.ReservedInstruction;
    }
}
