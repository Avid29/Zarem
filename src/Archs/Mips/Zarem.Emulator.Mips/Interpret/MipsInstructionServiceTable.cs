// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Extensions;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="MipsExecution{T}"/> models.
/// </summary>
public unsafe partial class MipsInstructionServiceTable<T, TS> : LogicTable, IMipsInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    // Main tables
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _opCodeTable = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[64];
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _specialTable = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[64];
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _special2Table = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[64];
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[] _regImmTable = new delegate*<MipsInstructionServiceTable<T, TS>, MipsInstruction, out MipsExecution<T>, MipsTrap>[32];

    // CoProcessor tables
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, FloatInstruction, out MipsExecution<T>, MipsTrap>[] _coProc1RSTable = new delegate*<MipsInstructionServiceTable<T, TS>, FloatInstruction, out MipsExecution<T>, MipsTrap>[32];
    private readonly delegate*<MipsInstructionServiceTable<T, TS>, FloatInstruction, out MipsExecution<T>, MipsTrap>[][] _floatFuncTables;

    private readonly MipsCpu<T> _processor;
    private readonly T* _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionServiceTable{T, TSigned}"/> struct.
    /// </summary>
    public MipsInstructionServiceTable(MipsCpu<T> processor)
    {
        _processor = processor;
        _regs = processor.RegisterFile.Regs;

        var formatCount = processor.Config.Version.Is64Bit() ? 4 : 3;
        _floatFuncTables = new delegate*<MipsInstructionServiceTable<T, TS>, FloatInstruction, out MipsExecution<T>, MipsTrap>[formatCount][];

        InitTables(processor.Config);
    }

    /// <inheritdoc/>
    public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
    {
        var func = _opCodeTable[(int)instruction.OpCode];
        return func(this, instruction, out execution);
    }

    private static MipsTrap DispatchSpecial(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var func = @this._specialTable[(int)inst.FuncCode];
        return func(@this, inst, out exec);
    }

    private static MipsTrap DispatchSpecial2(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var func = @this._special2Table[(int)inst.FuncCode];
        return func(@this, inst, out exec);
    }

    private static MipsTrap DispatchRegImm(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var func = @this._regImmTable[(int)inst.RTFuncCode];
        return func(@this, inst, out exec);
    }

    private static MipsTrap Shift<TLogic, TFormat>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, inst.ShiftAmount)));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftPlus32<TLogic, TFormat>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, inst.ShiftAmount + 32)));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftVar<TLogic, TFormat>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs = int.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, rs)));
        return MipsTrap.None;
    }

    private static MipsTrap AluR<TLogic, TFormat>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, INumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs, rt)));
        return MipsTrap.None;
    }

    private static MipsTrap AluR<TLogic, TFormat, TSize>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, INumber<TFormat>
        where TSize : unmanaged, IUnsignedNumber<TSize>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        var result = T.CreateTruncating(TSize.CreateTruncating(TLogic.Compute(rs, rt)));
        exec = MipsExecution<T>.CreateWriteback(inst.RD, result);
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluR<TLogic, TFormat, TSize>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICheckedAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
        where TSize : unmanaged, IBinaryInteger<TSize>, IUnsignedNumber<TSize>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        var result = TLogic.Compute(rs, rt);

        if (TLogic.Overflow(rs, rt, result))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        var value = T.CreateTruncating(TSize.CreateTruncating(result));
        exec = MipsExecution<T>.CreateWriteback(inst.RD, value);
        return MipsTrap.None;
    }

    private static MipsTrap AluI<TLogic, TFormat>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var imm = TFormat.CreateTruncating(inst.Immediate);
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(TLogic.Compute(rs, imm)));
        return MipsTrap.None;
    }

    private static MipsTrap AluISigned<TLogic, TFormat>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var imm = TFormat.CreateSaturating(inst.Immediate);
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(TLogic.Compute(rs, imm)));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluI<TLogic, TFormat, TSize>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICheckedAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
        where TSize : unmanaged, IBinaryInteger<TSize>, IUnsignedNumber<TSize>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var imm = TFormat.CreateSaturating(inst.Immediate);
        var result = TLogic.Compute(rs, imm);

        if (TLogic.Overflow(rs, imm, result))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        var value = T.CreateTruncating(result);
        exec = MipsExecution<T>.CreateWriteback(inst.RT, value);
        return MipsTrap.None;
    }

    private static MipsTrap MultR<TLogic, TFormat, TLong>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultLogic<TFormat, TLong>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        TLong value = TLogic.Compute(rs, rt);

        int shift = sizeof(TFormat) * 8;
        TLong mask = TLong.CreateTruncating(TFormat.AllBitsSet);

        T hi = T.CreateTruncating(value >> shift);
        T low = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((hi, low));
        return MipsTrap.None;
    }

    private static MipsTrap MultAddR<TLogic, TFormat, TLong>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultAddLogic<TFormat, TLong>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);

        int shift = sizeof(TFormat) * 8;
        TLong mask = TLong.CreateTruncating(TFormat.AllBitsSet);

        TLong hiPart = TLong.CreateTruncating(@this._processor.RegisterFile.High) << shift;
        TLong loPart = TLong.CreateTruncating(@this._processor.RegisterFile.Low) & mask;
        TLong @base = hiPart | loPart;

        TLong value = TLogic.Compute(rs, rt, @base);

        T outHi = T.CreateTruncating(value >> shift);
        T outLow = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((outHi, outLow));
        return MipsTrap.None;
    }

    private static MipsTrap DivR<TLogic, TFormat>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IDivLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        var rem = T.CreateTruncating(TLogic.Remainder(rs, rt));
        var div = T.CreateTruncating(TLogic.Divisor(rs, rt));
        exec = MipsExecution<T>.CreateHighLow((rem, div));
        return MipsTrap.None;
    }

    private static MipsTrap DivR<TLogic, TFormat, TSize>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IDivLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
        where TSize : unmanaged, IUnsignedNumber<TSize>
    {
        var rs = TFormat.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(@this._regs[(int)inst.RT]);
        var rem = T.CreateTruncating(TSize.CreateTruncating(TLogic.Remainder(rs, rt)));
        var div = T.CreateTruncating(TSize.CreateTruncating(TLogic.Divisor(rs, rt)));
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

    private static MipsTrap BranchOnLikely<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICondLogic<T>
    {
        var rs = T.CreateTruncating(@this._processor[inst.RS]);
        var rt = T.CreateTruncating(@this._processor[inst.RT]);

        // PC + 4 is the Delay Slot. inst.Offset is relative to the Delay Slot.
        var jumpTarget = @this._processor.ProgramCounter + T.CreateTruncating(inst.Offset + 4);

        if (TLogic.Check(rs, rt))
        {
            // Branch Taken: Execute delay slot, then jump.
            exec = MipsExecution<T>.CreateJump(jumpTarget);
        }
        else if (!@this._processor.Config.DisableDelaySlots)
        {
            // Branch NOT Taken: Nullify (skip) the delay slot.
            // We force a jump to PC + 8 to bypass the delay slot entirely.
            var skipDelaySlot = @this._processor.ProgramCounter + T.CreateTruncating(8);
            exec = MipsExecution<T>.CreateJump(skipDelaySlot, force: true);
        }
        else
        {
            exec = default;
        }

        return MipsTrap.None;
    }

    private static MipsTrap BranchLinkOnLikely<TLogic>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(@this._processor[inst.RS]);
        var rt = T.CreateTruncating(@this._processor[inst.RT]);

        var jumpTarget = @this._processor.ProgramCounter + T.CreateTruncating(inst.Offset + 4);
        var linkAddr = @this._processor.ProgramCounter + T.CreateTruncating(8);

        if (TLogic.Check(rs, rt))
        {
            // Taken: Link, execute delay slot, then jump.
            exec = MipsExecution<T>.CreateJumpAndLink(jumpTarget, linkAddr);
        }
        else if (!@this._processor.Config.DisableDelaySlots)
        {
            // NOT Taken: Skip delay slot. No linking occurs on failed Branch Likely.
            var skipDelaySlot = @this._processor.ProgramCounter + T.CreateTruncating(8);
            exec = MipsExecution<T>.CreateJump(skipDelaySlot, force: true);
        }
        else
        {
            exec = default;
        }

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
        var rs = T.CreateTruncating(@this._regs[(int)inst.RS]);
        var rt = T.CreateTruncating(@this._regs[(int)inst.RT]);
        exec = TLogic.Check(rs, rt) ? MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(rs)) : default;
        return MipsTrap.None;
    }

    private static MipsTrap Load<TData>(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        T offset = T.CreateTruncating(inst.Immediate);
        T baseAddr = T.CreateTruncating(@this._regs[(int)inst.RS]);
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
        T baseAddr = T.CreateTruncating(@this._regs[(int)inst.RS]);
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
        var linkOffset = @this._processor.Config.DisableDelaySlots ? T.CreateTruncating(4) : T.CreateTruncating(8);
        exec = MipsExecution<T>.CreateJumpAndLink(T.CreateTruncating(inst.Address), @this._processor.ProgramCounter + linkOffset);
        return MipsTrap.None;
    }

    private static MipsTrap JumpR(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateJump(@this._regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap JumpLinkR(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var linkOffset = @this._processor.Config.DisableDelaySlots ? T.CreateTruncating(4) : T.CreateTruncating(8);
        var rs = @this._regs[(int)inst.RS];
        exec = MipsExecution<T>.CreateJumpAndLink(rs, @this._processor.ProgramCounter + linkOffset, inst.RD);
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

    private static MipsTrap ReservedInstruction(MipsInstructionServiceTable<T, TS> @this, FloatInstruction inst, out MipsExecution<T> exec)
        => ReservedInstruction(@this, (MipsInstruction)inst, out exec);

    private static MipsTrap NotImplemented(MipsInstructionServiceTable<T, TS> @this, MipsInstruction inst, out MipsExecution<T> exec)
        => throw new UnimplementedInstructionException(ulong.CreateTruncating(@this._processor.ProgramCounter));
}
