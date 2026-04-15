// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="MipsExecution{T}"/> models.
/// </summary>
public unsafe partial class MipsInstructionServiceTable<T, TS> : LogicTable, IMipsInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _opCodeTable = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[64];
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _specialTable = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[64];
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _special2Table = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[64];
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _regImmTable = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[32];
    private readonly MipsCpu<T> _processor;
    private readonly T* _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionServiceTable{T, TSigned}"/> struct.
    /// </summary>
    public MipsInstructionServiceTable(MipsCpu<T> processor)
    {
        _processor = processor;
        _regs = processor.RegisterFile.Regs;

        InitTables(processor.Config);
    }

    /// <inheritdoc/>
    public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
    {
        var func = _opCodeTable[(int)instruction.OpCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(this, instruction, out execution);
    }

    private static MipsTrap DispatchSpecial(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var func = @this._specialTable[(int)inst.FuncCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(@this, inst, out exec);
    }

    private static MipsTrap DispatchSpecial2(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var func = @this._special2Table[(int)inst.FuncCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(@this, inst, out exec);
    }

    private static MipsTrap DispatchRegImm(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var func = @this._regImmTable[(int)inst.RTFuncCode];
        if (func == null)
        {
            throw new NotImplementedException();
        }

        return func(@this, inst, out exec);
    }

    private static MipsTrap Shift<TLogic, T2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rt = T2.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, inst.ShiftAmount)));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftPlus32<TLogic, T2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rt = T2.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, inst.ShiftAmount + 32)));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftVar<TLogic, T2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs = int.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = T2.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, rs)));
        return MipsTrap.None;
    }

    private static MipsTrap AluR<TLogic, T2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs = T2.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = T2.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs, rt)));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluR<TLogic, T2, TSigned2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICheckedAluLogic<T2, TSigned2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TSigned2 : unmanaged, IBinaryInteger<TSigned2>, ISignedNumber<TSigned2>
    {
        var rs = T2.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = T2.CreateTruncating(@this._regs[(int)inst.RT]);
        var value = TLogic.Compute(rs, rt);

        if (TLogic.Overflow(TSigned2.CreateTruncating(rs), TSigned2.CreateTruncating(rt), TSigned2.CreateTruncating(value)))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(value));
        return MipsTrap.None;
    }

    private static MipsTrap AluI<TLogic, T2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs = T2.CreateTruncating(@this._regs[(int)inst.RS]);
        var imm = T2.CreateTruncating(inst.Immediate);
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(TLogic.Compute(rs, imm)));
        return MipsTrap.None;
    }

    private static MipsTrap AluISigned<TLogic, T2, TSigned2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TSigned2 : unmanaged, IBinaryInteger<TSigned2>, ISignedNumber<TSigned2>
    {
        var rs = T2.CreateTruncating(@this._regs[(int)inst.RS]);
        var imm = T2.CreateTruncating(TSigned2.CreateSaturating(inst.Immediate));
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(TLogic.Compute(rs, imm)));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluI<TLogic, T2, TSigned2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICheckedAluLogic<T2, TSigned2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TSigned2 : unmanaged, IBinaryInteger<TSigned2>, ISignedNumber<TSigned2>
    {
        var rs = T2.CreateTruncating(@this._regs[(int)inst.RS]);
        var imm = T2.CreateTruncating(TS.CreateSaturating(inst.Immediate));
        var value = TLogic.Compute(rs, imm);

        if (TLogic.Overflow(TSigned2.CreateTruncating(rs), TSigned2.CreateTruncating(imm), TSigned2.CreateTruncating(value)))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(value));
        return MipsTrap.None;
    }

    private static MipsTrap MultR<TLogic, T2, TL2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultLogic<T2, TL2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TL2 : unmanaged, IBinaryInteger<TL2>, IUnsignedNumber<TL2>
    {
        var rs = T2.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = T2.CreateTruncating(@this._regs[(int)inst.RT]);
        TL2 value = TLogic.Compute(rs, rt);

        int shift = sizeof(T2) * 8;
        TL2 mask = TL2.CreateTruncating(T2.AllBitsSet);

        T hi = T.CreateTruncating(value >> shift);
        T low = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((hi, low));
        return MipsTrap.None;
    }

    private static MipsTrap MultAddR<TLogic, T2, TL2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultAddLogic<T2, TL2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TL2 : unmanaged, IBinaryInteger<TL2>, IUnsignedNumber<TL2>
    {
        var rs = T2.CreateTruncating(@this._processor[inst.RS]);
        var rt = T2.CreateTruncating(@this._processor[inst.RT]);

        int shift = sizeof(T2) * 8;
        TL2 mask = TL2.CreateTruncating(T2.AllBitsSet);

        TL2 hiPart = TL2.CreateTruncating(@this._processor.RegisterFile.High) << shift;
        TL2 loPart = TL2.CreateTruncating(@this._processor.RegisterFile.Low) & mask;
        TL2 @base = hiPart | loPart;

        TL2 value = TLogic.Compute(rs, rt, @base);

        T outHi = T.CreateTruncating(value >> shift);
        T outLow = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((outHi, outLow));
        return MipsTrap.None;
    }

    private static MipsTrap DivR<TLogic, T2>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IDivLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs = T2.CreateTruncating(@this._processor[inst.RS]);
        var rt = T2.CreateTruncating(@this._processor[inst.RT]);
        var rem = T.CreateTruncating(TLogic.Remainder(rs, rt));
        var div = T.CreateTruncating(TLogic.Divisor(rs, rt));
        exec = MipsExecution<T>.CreateHighLow((rem, div));
        return MipsTrap.None;
    }

    private static MipsTrap Trap<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ITrapLogic
    {
        exec = default;
        return TLogic.Trap();
    }

    private static MipsTrap BranchOn<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICondLogic<T>
    {
        var rs = T.CreateTruncating(@this._processor[inst.RS]);
        var rt = T.CreateTruncating(@this._processor[inst.RT]);
        var jump = @this._processor.ProgramCounter + T.CreateTruncating(inst.Offset + 4);
        exec = TLogic.Check(rs, rt) ? MipsExecution<T>.CreateJump(jump) : default;
        return MipsTrap.None;
    }

    private static MipsTrap BranchLinkOn<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(@this._processor[inst.RS]);
        var rt = T.CreateTruncating(@this._processor[inst.RT]);
        var jump = @this._processor.ProgramCounter + T.CreateTruncating(inst.Offset + 4);
        var ret = @this._processor.ProgramCounter + T.CreateTruncating(4);
        exec = TLogic.Check(rs, rt) ? MipsExecution<T>.CreateJumpAndLink(jump, ret) : default;
        return MipsTrap.None;
    }

    private static MipsTrap TrapOn<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(@this._processor[inst.RS]);
        var rt = T.CreateTruncating(@this._processor[inst.RT]);
        exec = default;
        return TLogic.Check(rs, rt) ? MipsTrap.Trap : MipsTrap.None;
    }

    private static MipsTrap TrapOnI<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(@this._processor[inst.RS]);
        var imm = T.CreateTruncating(TS.CreateSaturating(inst.Immediate));
        exec = default;
        return TLogic.Check(rs, imm) ? MipsTrap.Trap : MipsTrap.None;
    }

    private static MipsTrap Move<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(@this._processor[inst.RS]);
        var rt = T.CreateTruncating(@this._processor[inst.RT]);
        exec = TLogic.Check(rs, rt) ? MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(rs)) : default;
        return MipsTrap.None;
    }

    private static MipsTrap Load<TData>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        T offset = T.CreateTruncating(inst.Immediate);
        T baseAddr = T.CreateTruncating(@this._processor[inst.RS]);
        T addr = baseAddr + offset;

        // Alignment check (bytes are always aligned)
        int size = sizeof(TData);
        if (size > 1 && (addr & T.CreateTruncating(size - 1)) != T.Zero)
        {
            exec = default;
            return MipsTrap.AddressErrorLoad;
        }

        bool signed = typeof(TData) == typeof(sbyte) || typeof(TData) == typeof(short) || typeof(TData) == typeof(int)|| typeof(TData) == typeof(long);
        exec = MipsExecution<T>.CreateMemRead(inst.RT, addr, size, signed);
        return MipsTrap.None;
    }

    private static MipsTrap Store<TData>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TData : unmanaged
    {
        T offset = T.CreateTruncating(inst.Immediate);
        T baseAddr = T.CreateTruncating(@this._processor[inst.RS]);
        T addr = baseAddr + offset;

        // Alignment check (bytes are always aligned)
        int size = sizeof(TData);
        if (size > 1 && (addr & T.CreateTruncating(size - 1)) != T.Zero)
        {
            exec = default;
            return MipsTrap.AddressErrorStore;
        }

        exec = MipsExecution<T>.CreateMemWrite(@this._regs[(int)inst.RT], addr, size);
        return MipsTrap.None;
    }

    private static MipsTrap Jump(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateJump(T.CreateTruncating(inst.Address));
        return MipsTrap.None;
    }

    private static MipsTrap JumpLink(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateJumpAndLink(T.CreateTruncating(inst.Address), @this._processor.ProgramCounter + T.CreateTruncating(4));
        return MipsTrap.None;
    }

    private static MipsTrap JumpR(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateJump(@this._regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap JumpLinkR(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var rs = @this._regs[(int)inst.RS];
        exec = MipsExecution<T>.CreateJumpAndLink(rs, @this._processor.ProgramCounter + T.CreateTruncating(4), inst.RD);
        return MipsTrap.None;
    }

    private static MipsTrap Mfhi(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RD, @this._processor.RegisterFile.High);
        return MipsTrap.None;
    }

    private static MipsTrap Mthi(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateHigh(@this._regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap Mflo(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RD, @this._processor.RegisterFile.Low);
        return MipsTrap.None;
    }

    private static MipsTrap Mtlo(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateLow(@this._regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap Lui(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(inst.Immediate << 16));
        return MipsTrap.None;
    }

    private static MipsTrap ReservedInstruction(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = default;
        return MipsTrap.ReservedInstruction;
    }

    private static MipsTrap NotImplemented(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        => throw new UnimplementedInstructionException(ulong.CreateTruncating(@this._processor.ProgramCounter));
}
