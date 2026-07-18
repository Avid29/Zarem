// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="MipsExecution{T}"/> models.
/// </summary>
public unsafe partial class MipsInstructionServiceTable<T, TS> : IMipsInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    private readonly MipsInstructionDecodeTable<IntPtr> _instructionTable;
    private readonly MipsInterpretCpu<T> _cpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionServiceTable{T, TSigned}"/> struct.
    /// </summary>
    public MipsInstructionServiceTable(MipsInterpretCpu<T> cpu)
    {
        _cpu = cpu;

        _instructionTable = new MipsInstructionDecodeTable<IntPtr>(GetFunctionPtrValue(&ReservedInstruction));

        InitTables(cpu.Config);
    }

    /// <inheritdoc/>
    public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
    {
        var func = (delegate*<MipsInterpretCpu<T>, MipsInstruction, out MipsExecution<T>, MipsTrap>)_instructionTable.Lookup(instruction);
        return func(_cpu, instruction, out execution);
    }

    private static MipsTrap Shift<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, inst.ShiftAmount)));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftPlus32<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, inst.ShiftAmount + 32)));
        return MipsTrap.None;
    }

    private static MipsTrap ShiftVar<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs = int.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rt, rs)));
        return MipsTrap.None;
    }

    private static MipsTrap AluR<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, INumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        exec = MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs, rt)));
        return MipsTrap.None;
    }

    private static MipsTrap SignedAluR<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, INumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        var result = T.CreateTruncating(TS.CreateTruncating(TLogic.Compute(rs, rt)));
        exec = MipsExecution<T>.CreateWriteback(inst.RD, result);
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluR<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICheckedAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        var result = TLogic.Compute(rs, rt);

        if (TLogic.Overflow(rs, rt, result))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        var value = T.CreateTruncating(TS.CreateTruncating(result));
        exec = MipsExecution<T>.CreateWriteback(inst.RD, value);
        return MipsTrap.None;
    }

    private static MipsTrap AluI<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var imm = TFormat.CreateTruncating(inst.Immediate);
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(TLogic.Compute(rs, imm)));
        return MipsTrap.None;
    }

    private static MipsTrap AluISigned<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var imm = TFormat.CreateSaturating(inst.Immediate);
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(TLogic.Compute(rs, imm)));
        return MipsTrap.None;
    }

    private static MipsTrap CheckedAluI<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICheckedAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var imm = TFormat.CreateTruncating(inst.Immediate);
        var result = TLogic.Compute(rs, imm);

        if (TLogic.Overflow(rs, imm, result))
        {
            exec = default;
            return MipsTrap.ArithmeticOverflow;
        }

        var value = T.CreateTruncating(TS.CreateTruncating(result));
        exec = MipsExecution<T>.CreateWriteback(inst.RT, value);
        return MipsTrap.None;
    }

    private static MipsTrap MultR<TLogic, TFormat, TLong>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultLogic<TFormat, TLong>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        TLong value = TLogic.Compute(rs, rt);

        int shift = sizeof(TFormat) * 8;
        TLong mask = TLong.CreateTruncating(TFormat.AllBitsSet);

        T hi = T.CreateTruncating(value >> shift);
        T low = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((hi, low));
        return MipsTrap.None;
    }

    private static MipsTrap SignedMultR<TLogic, TFormat, TLong>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultLogic<TFormat, TLong>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
        where TLong : unmanaged, IBinaryInteger<TLong>, ISignedNumber<TLong>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        TLong value = TLogic.Compute(rs, rt);

        int shift = sizeof(TFormat) * 8;
        TLong mask = (TLong.One << shift) - TLong.One;

        T hi = T.CreateTruncating(TS.CreateTruncating(value >> shift));
        T low = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((hi, low));
        return MipsTrap.None;
    }

    private static MipsTrap MultAddR<TLogic, TFormat, TLong>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultAddLogic<TFormat, TLong>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);

        int shift = sizeof(TFormat) * 8;
        TLong mask = TLong.CreateTruncating(TFormat.AllBitsSet);

        TLong hiPart = TLong.CreateTruncating(cpu.RegisterFile.High) << shift;
        TLong loPart = TLong.CreateTruncating(cpu.RegisterFile.Low) & mask;
        TLong @base = hiPart | loPart;

        TLong value = TLogic.Compute(rs, rt, @base);

        T outHi = T.CreateTruncating(value >> shift);
        T outLow = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((outHi, outLow));
        return MipsTrap.None;
    }

    private static MipsTrap SignedMultAddR<TLogic, TFormat, TLong>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IMultAddLogic<TFormat, TLong>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
        where TLong : unmanaged, IBinaryInteger<TLong>, ISignedNumber<TLong>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);

        int shift = sizeof(TFormat) * 8;
        TLong mask = (TLong.One << shift) - TLong.One;

        TLong hiPart = TLong.CreateTruncating(cpu.RegisterFile.High) << shift;
        TLong loPart = TLong.CreateTruncating(cpu.RegisterFile.Low) & mask;
        TLong @base = hiPart | loPart;

        TLong value = TLogic.Compute(rs, rt, @base);

        T outHi = T.CreateTruncating(TS.CreateTruncating(value >> shift));
        T outLow = T.CreateTruncating(value & mask);

        exec = MipsExecution<T>.CreateHighLow((outHi, outLow));
        return MipsTrap.None;
    }

    private static MipsTrap DivR<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IDivLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        var rem = T.CreateTruncating(TLogic.Remainder(rs, rt));
        var div = T.CreateTruncating(TLogic.Divisor(rs, rt));
        exec = MipsExecution<T>.CreateHighLow((rem, div));
        return MipsTrap.None;
    }

    private static MipsTrap SignedDivR<TLogic, TFormat>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, IDivLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        var rem = T.CreateTruncating(TS.CreateTruncating(TLogic.Remainder(rs, rt)));
        var div = T.CreateTruncating(TS.CreateTruncating(TLogic.Divisor(rs, rt)));
        exec = MipsExecution<T>.CreateHighLow((rem, div));
        return MipsTrap.None;
    }

    private static MipsTrap Trap<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ITrapLogic
    {
        exec = default;
        return TLogic.Trap();
    }

    private static MipsTrap BranchOn<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICondLogic<T>
    {
        var rs = T.CreateTruncating(cpu[inst.RS]);
        var rt = T.CreateTruncating(cpu[inst.RT]);
        var jump = cpu.ProgramCounter + T.CreateTruncating(inst.Offset + 4);
        exec = TLogic.Check(rs, rt) ? MipsExecution<T>.CreateJump(jump) : default;
        return MipsTrap.None;
    }

    private static MipsTrap BranchLinkOn<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(cpu[inst.RS]);
        var rt = T.CreateTruncating(cpu[inst.RT]);
        var jump = cpu.ProgramCounter + T.CreateTruncating(inst.Offset + 4);
        var ret = cpu.ProgramCounter + T.CreateTruncating(4);
        exec = TLogic.Check(rs, rt) ? MipsExecution<T>.CreateJumpAndLink(jump, ret) : default;
        return MipsTrap.None;
    }

    private static MipsTrap BranchOnLikely<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : struct, ICondLogic<T>
    {
        var rs = T.CreateTruncating(cpu[inst.RS]);
        var rt = T.CreateTruncating(cpu[inst.RT]);

        // PC + 4 is the Delay Slot. inst.Offset is relative to the Delay Slot.
        var jumpTarget = cpu.ProgramCounter + T.CreateTruncating(inst.Offset + 4);

        if (TLogic.Check(rs, rt))
        {
            // Branch Taken: Execute delay slot, then jump.
            exec = MipsExecution<T>.CreateJump(jumpTarget);
        }
        else if (!cpu.Config.DisableDelaySlots)
        {
            // Branch NOT Taken: Nullify (skip) the delay slot.
            // We force a jump to PC + 8 to bypass the delay slot entirely.
            var skipDelaySlot = cpu.ProgramCounter + T.CreateTruncating(8);
            exec = MipsExecution<T>.CreateJump(skipDelaySlot, force: true);
        }
        else
        {
            exec = default;
        }

        return MipsTrap.None;
    }

    private static MipsTrap BranchLinkOnLikely<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(cpu[inst.RS]);
        var rt = T.CreateTruncating(cpu[inst.RT]);

        var jumpTarget = cpu.ProgramCounter + T.CreateTruncating(inst.Offset + 4);
        var linkAddr = cpu.ProgramCounter + T.CreateTruncating(8);

        if (TLogic.Check(rs, rt))
        {
            // Taken: Link, execute delay slot, then jump.
            exec = MipsExecution<T>.CreateJumpAndLink(jumpTarget, linkAddr);
        }
        else if (!cpu.Config.DisableDelaySlots)
        {
            // NOT Taken: Skip delay slot. No linking occurs on failed Branch Likely.
            var skipDelaySlot = cpu.ProgramCounter + T.CreateTruncating(8);
            exec = MipsExecution<T>.CreateJump(skipDelaySlot, force: true);
        }
        else
        {
            exec = default;
        }

        return MipsTrap.None;
    }

    private static MipsTrap TrapOn<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(cpu[inst.RS]);
        var rt = T.CreateTruncating(cpu[inst.RT]);
        exec = default;
        return TLogic.Check(rs, rt) ? MipsTrap.Trap : MipsTrap.None;
    }

    private static MipsTrap TrapOnI<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(cpu[inst.RS]);
        var imm = T.CreateTruncating(TS.CreateSaturating(inst.Immediate));
        exec = default;
        return TLogic.Check(rs, imm) ? MipsTrap.Trap : MipsTrap.None;
    }

    private static MipsTrap Move<TLogic>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TLogic : ICondLogic<T>
    {
        var rs = T.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        var rt = T.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RT]);
        exec = TLogic.Check(rs, rt) ? MipsExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(rs)) : default;
        return MipsTrap.None;
    }

    private static MipsTrap Load<TData>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        T offset = T.CreateTruncating(inst.Immediate);
        T baseAddr = T.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
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

    private static MipsTrap Store<TData>(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        where TData : unmanaged
    {
        T offset = T.CreateTruncating(inst.Immediate);
        T baseAddr = T.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS]);
        T addr = baseAddr + offset;

        // Alignment check (bytes are always aligned)
        int size = sizeof(TData);
        if (size > 1 && (addr & T.CreateTruncating(size - 1)) != T.Zero)
        {
            exec = default;
            return MipsTrap.AddressErrorStore;
        }

        exec = MipsExecution<T>.CreateMemWrite(cpu.RegisterFile.Regs[(int)inst.RT], addr, size);
        return MipsTrap.None;
    }

    private static MipsTrap Jump(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateJump(T.CreateTruncating(inst.Address));
        return MipsTrap.None;
    }

    private static MipsTrap JumpLink(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var linkOffset = cpu.Config.DisableDelaySlots ? T.CreateTruncating(4) : T.CreateTruncating(8);
        exec = MipsExecution<T>.CreateJumpAndLink(T.CreateTruncating(inst.Address), cpu.ProgramCounter + linkOffset);
        return MipsTrap.None;
    }

    private static MipsTrap JumpR(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateJump(cpu.RegisterFile.Regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap JumpLinkR(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        var linkOffset = cpu.Config.DisableDelaySlots ? T.CreateTruncating(4) : T.CreateTruncating(8);
        var rs = cpu.RegisterFile.Regs[(int)inst.RS];
        exec = MipsExecution<T>.CreateJumpAndLink(rs, cpu.ProgramCounter + linkOffset, inst.RD);
        return MipsTrap.None;
    }

    private static MipsTrap Mfhi(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RD, cpu.RegisterFile.High);
        return MipsTrap.None;
    }

    private static MipsTrap Mthi(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateHigh(cpu.RegisterFile.Regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap Mflo(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RD, cpu.RegisterFile.Low);
        return MipsTrap.None;
    }

    private static MipsTrap Mtlo(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateLow(cpu.RegisterFile.Regs[(int)inst.RS]);
        return MipsTrap.None;
    }

    private static MipsTrap Lui(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = MipsExecution<T>.CreateWriteback(inst.RT, T.CreateTruncating(inst.Immediate << 16));
        return MipsTrap.None;
    }

    private static MipsTrap ReservedInstruction(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
    {
        exec = default;
        return MipsTrap.ReservedInstruction;
    }

    private static MipsTrap NotImplemented(MipsInterpretCpu<T> cpu, MipsInstruction inst, out MipsExecution<T> exec)
        => throw new UnimplementedInstructionException(ulong.CreateTruncating(cpu.ProgramCounter));
}
