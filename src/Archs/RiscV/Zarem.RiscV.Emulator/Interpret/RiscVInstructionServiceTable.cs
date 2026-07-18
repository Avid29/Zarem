// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Exceptions;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="RiscVExecution{T}"/> models.
/// </summary>
public unsafe partial class RiscVInstructionServiceTable<T, TFloat, TSigned> : IRiscVInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
    where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
{
    private readonly RiscVInstructionDecodeTable<IntPtr> _instructionTable;
    private readonly RiscVInterpretCpu<T, TFloat> _cpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionServiceTable{T, TFloat, TSigned}"/> struct.
    /// </summary>
    public RiscVInstructionServiceTable(RiscVInterpretCpu<T, TFloat> cpu)
    {
        _cpu = cpu;

        _instructionTable = new RiscVInstructionDecodeTable<nint>(GetFunctionPtrValue(&IllegalInstruction));

        InitTables(cpu.Config);
    }

    /// <inheritdoc/>
    public RiscVTrap Execute(RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        var func = (delegate*<RiscVInterpretCpu<T, TFloat>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>)_instructionTable.Lookup(inst);
        return func(_cpu, inst, out exec);
    }

    private static RiscVTrap AluR<TLogic, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs1 = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS1]);
        var rs2 = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS2]);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs1, rs2)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ModifyableAluR<TBase, TMod, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TBase : struct, IAluLogic<TFormat>
        where TMod : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
        => inst.Funct7 is Funct7Code.Modified ? AluR<TMod, TFormat>(cpu, inst, out exec) : AluR<TBase, TFormat>(cpu, inst, out exec);

    private static RiscVTrap AluI<TLogic, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, IUnsignedNumber<TFormat>
    {
        var rs1 = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS1]);
        var imm = TFormat.CreateTruncating(inst.Immediate);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs1, imm)));
        return RiscVTrap.None;
    }

    private static RiscVTrap AluISigned<TLogic, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>, ISignedNumber<TFormat>
    {
        var rs1 = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS1]);
        var imm = TFormat.CreateSaturating(inst.Immediate);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs1, imm)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ShiftR<TLogic, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs1 = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS1]);
        var rs2 = int.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS2]);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rs1, rs2)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ModifyableShiftR<TBase, TMod, TBaseFormat, TModFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TBase : struct, IShiftLogic<TBaseFormat>
        where TMod : struct, IShiftLogic<TModFormat>
        where TBaseFormat : unmanaged, IBinaryInteger<TBaseFormat>
        where TModFormat : unmanaged, IBinaryInteger<TModFormat>
        => inst.Funct7 is Funct7Code.Modified ? ShiftR<TMod, TModFormat>(cpu, inst, out exec) : ShiftR<TBase, TBaseFormat>(cpu, inst, out exec);

    private static RiscVTrap ShiftI<TLogic, TFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IShiftLogic<TFormat>
        where TFormat : unmanaged, IBinaryInteger<TFormat>
    {
        var rs1 = TFormat.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS1]);
        var imm = int.CreateTruncating(inst.Immediate) & (sizeof(TFormat) * 8 - 1);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rs1, imm)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ModifyableShiftI<TBase, TMod, TBaseFormat, TModFormat>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TBase : struct, IShiftLogic<TBaseFormat>
        where TMod : struct, IShiftLogic<TModFormat>
        where TBaseFormat : unmanaged, IBinaryInteger<TBaseFormat>
        where TModFormat : unmanaged, IBinaryInteger<TModFormat>
        => inst.Funct7 is Funct7Code.Modified ? ShiftI<TMod, TModFormat>(cpu, inst, out exec) : ShiftI<TBase, TBaseFormat>(cpu, inst, out exec);

    private static RiscVTrap JumpAndLink(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        var offset = T.CreateTruncating(inst.JumpOffset);
        var target = (cpu.ProgramCounter + offset) & ~T.One;
        var link = cpu.ProgramCounter + T.CreateTruncating(4);

        exec = RiscVExecution<T>.CreateJumpAndLink(target, link, inst.RD);
        return RiscVTrap.None;
    }

    private static RiscVTrap JumpAndLinkRegister(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        var @base = T.CreateTruncating(cpu[inst.RS1]);
        var offset = T.CreateTruncating(inst.Immediate);
        var target = @base + offset;
        var link = cpu.ProgramCounter + T.CreateTruncating(4);

        exec = RiscVExecution<T>.CreateJumpAndLink(target, link, inst.RD);
        return RiscVTrap.None;
    }

    private static RiscVTrap BranchOn<TLogic>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, ICondLogic<T>
    {
        var rs1 = T.CreateTruncating(cpu[inst.RS1]);
        var rs2 = T.CreateTruncating(cpu[inst.RS2]);
        var jump = cpu.ProgramCounter + T.CreateTruncating(inst.BranchOffset);
        exec = TLogic.Check(rs1, rs2) ? RiscVExecution<T>.CreateJump(jump) : default;
        return RiscVTrap.None;
    }

    private static RiscVTrap Load<TData>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        T offset = T.CreateTruncating(inst.Immediate);
        T baseAddr = T.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS1]);
        T addr = baseAddr + offset;

        // Alignment check (bytes are always aligned)
        int size = sizeof(TData);
        if (size > 1 && (addr & T.CreateTruncating(size - 1)) != T.Zero)
        {
            exec = default;
            return RiscVTrap.LoadAddressMisaligned;
        }

        bool signed = typeof(TData) == typeof(sbyte) || typeof(TData) == typeof(short) || typeof(TData) == typeof(int) || typeof(TData) == typeof(long);
        exec = RiscVExecution<T>.CreateMemRead(inst.RD, addr, size, signed);
        return RiscVTrap.None;
    }

    private static RiscVTrap Store<TData>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TData : unmanaged
    {
        T offset = T.CreateTruncating(inst.StoreOffset);
        T baseAddr = T.CreateTruncating(cpu.RegisterFile.Regs[(int)inst.RS1]);
        T addr = baseAddr + offset;

        // Alignment check (bytes are always aligned)
        int size = sizeof(TData);
        if (size > 1 && (addr & T.CreateTruncating(size - 1)) != T.Zero)
        {
            exec = default;
            return RiscVTrap.StoreAddressMisaligned;
        }

        exec = RiscVExecution<T>.CreateMemWrite(cpu.RegisterFile.Regs[(int)inst.RS2], addr, size);
        return RiscVTrap.None;
    }

    private static RiscVTrap EcallBreak(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        exec = default;
        return inst.Immediate is 1 ? RiscVTrap.Breakpoint : RiscVTrap.EnvironmentCallFromUMode;
    }

    private static RiscVTrap Lui(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(inst.Immediate << 12));
        return RiscVTrap.None;
    }

    private static RiscVTrap IllegalInstruction(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        exec = default;
        return RiscVTrap.IllegalInstruction;
    }

    private static RiscVTrap NotImplemented(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, out RiscVExecution<T> exec)
        => throw new UnimplementedInstructionException(ulong.CreateTruncating(cpu.ProgramCounter));
}
