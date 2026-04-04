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
    private readonly delegate*<RiscVInstructionServiceTable<T, TS>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _modifiedTable = new delegate*<RiscVInstructionServiceTable<T, TS>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[1024];
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
    public RiscVTrap Execute(RiscVInstruction instruction, out RiscVExecution<T> execution)
        => _funct7Table[(int)instruction.OpCode](this, instruction, out execution);

    private static RiscVTrap DispatchBaseTable(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        => @this._baseTable[GetLookupIndex(inst)](@this, inst, out exec);

    private static RiscVTrap DispatchModifiedTable(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        => @this._modifiedTable[GetLookupIndex(inst)](@this, inst, out exec);

    private static RiscVTrap AluR<TLogic, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var rs2 = T2.CreateTruncating(@this._regs[(int)inst.RS2]);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Compute(rs1, rs2)));
        return RiscVTrap.None;
    }

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

    private static RiscVTrap Shift<TLogic, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var imm = int.CreateTruncating(inst.Immediate) & (sizeof(T2) * 8 - 1);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rs1, imm)));
        return RiscVTrap.None;
    }

    private static RiscVTrap ShiftVar<TLogic, T2>(RiscVInstructionServiceTable<T, TS> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IShiftLogic<T2>
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        var rs1 = T2.CreateTruncating(@this._regs[(int)inst.RS1]);
        var rs2 = int.CreateTruncating(@this._regs[(int)inst.RS2]);
        exec = RiscVExecution<T>.CreateWriteback(inst.RD, T.CreateTruncating(TLogic.Execute(rs1, rs2)));
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
