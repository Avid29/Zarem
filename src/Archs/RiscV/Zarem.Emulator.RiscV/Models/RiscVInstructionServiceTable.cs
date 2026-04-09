// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="RiscVExecution{T}"/> models.
/// </summary>
public unsafe partial class RiscVInstructionServiceTable<T, TS> : LogicTable, IRiscVInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TS : unmanaged, IBinaryInteger<TS>, ISignedNumber<TS>
{
    private readonly delegate*<RiscVInstructionServiceTable<T, TS>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _funct7Table = new delegate*<RiscVInstructionServiceTable<T, TS>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[128];
    private readonly delegate*<RiscVInstructionServiceTable<T, TS>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _baseTable = new delegate*<RiscVInstructionServiceTable<T, TS>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[1024];
    private readonly RiscVCpu<T> _processor;
    private readonly T* _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionServiceTable{T, TSigned}"/> struct.
    /// </summary>
    public RiscVInstructionServiceTable(RiscVCpu<T> cpu)
    {
        _processor = cpu;
        _regs = cpu.RegisterFile.Regs;

        InitTables(cpu.Config);
    }

    /// <inheritdoc/>
    public RiscVTrap Execute(RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        if (inst.OpCode is OperationCode.Alu or OperationCode.Alu32 or OperationCode.Alu64)
            return _funct7Table[(int)inst.Funct7](this, inst, out exec);

        return DispatchBaseTable(this, inst, out exec);
    }

    private static RiscVTrap DispatchBaseTable(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        => @this._baseTable[GetLookupIndex(inst)](@this, inst, out exec);

    private static RiscVTrap AluR<TLogic, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var rs2 = T2.CreateTruncating(@this._regs[(int)inst.RS2]);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs1, rs2)));
        return RiscVTrap.None;
    }
    
    private static RiscVTrap ModifyableAluR<TBase, TMod, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TBase : struct, IAluLogic<T2>
        where TMod : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        => inst.Funct7 is Funct7Code.Modified ? AluR<TMod, T2>(@this, inst, out exec) : AluR<TBase, T2>(@this, inst, out exec);

    private static RiscVTrap AluI<TLogic, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var imm = T2.CreateTruncating(inst.Immediate);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs1, imm)));
        return RiscVTrap.None;
    }

    private static RiscVTrap AluISigned<TLogic, T2, TSigned2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TSigned2 : unmanaged, IBinaryInteger<TSigned2>, ISignedNumber<TSigned2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var imm = T2.CreateTruncating(TSigned2.CreateSaturating(inst.Immediate));
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs1, imm)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ShiftR<TLogic, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var rs2 = int.CreateTruncating(@this._regs[(int)inst.RS2]);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rs1, rs2)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ModifyableShiftR<TBase, TMod, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TBase : struct, IShiftLogic<T2>
        where TMod : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        => inst.Funct7 is Funct7Code.Modified ? ShiftR<TMod, T2>(@this, inst, out exec) : ShiftR<TBase, T2>(@this, inst, out exec);

    private static RiscVTrap ShiftI<TLogic, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var imm = int.CreateTruncating(inst.Immediate) & (sizeof(T2) * 8 - 1);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rs1, imm)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ModifyableShiftI<TBase, TMod, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TBase : struct, IShiftLogic<T2>
        where TMod : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        => inst.Funct7 is Funct7Code.Modified ? ShiftI<TMod, T2>(@this, inst, out exec) : ShiftI<TBase, T2>(@this, inst, out exec);

    private static RiscVTrap JumpAndLink(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        var jump = T.CreateTruncating(inst.JumpOffset);
        exec = RiscVExecution<T>.CreateJumpAndLink(jump, @this._processor.ProgramCounter + T.CreateTruncating(4), inst.RD);
        return RiscVTrap.None;
    }

    private static RiscVTrap BranchOn<TLogic>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, ICondLogic<T>
    {
        var rs1 = T.CreateTruncating(@this._processor[inst.RS1]);
        var rs2 = T.CreateTruncating(@this._processor[inst.RS2]);
        var jump = @this._processor.ProgramCounter + T.CreateTruncating(inst.BranchOffset) + T.CreateTruncating(4);
        exec = TLogic.Check(rs1, rs2) ? RiscVExecution<T>.CreateJump(jump) : default;
        return RiscVTrap.None;
    }

    private static RiscVTrap IllegalInstruction(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        exec = default;
        return RiscVTrap.IllegalInstruction;
    }

    private static RiscVTrap NotImplemented(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        => throw new UnimplementedInstructionException(ulong.CreateTruncating(@this._processor.ProgramCounter));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVInstruction instruction)
        => GetLookupIndex(instruction.OpCode, instruction.Funct3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(OperationCode op, Funct3Code funct3)
        => (int)op << 3 | (int)funct3;
}
