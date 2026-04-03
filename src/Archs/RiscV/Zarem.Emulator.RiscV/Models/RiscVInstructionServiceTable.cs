// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="RiscVExecution{T}"/> models.
/// </summary>
public unsafe partial class RiscVInstructionServiceTable<T, TSigned> : LogicTable<T, TSigned>, IRiscVInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
{
    private readonly delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _opCodeTable = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[32];
    private readonly delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _aluImmTable = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[8]; // Funct3
    private readonly delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _aluRegTable = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[8]; // Funct3
    private readonly delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _loadTable = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[8];  // Funct3
    private readonly delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _storeTable = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[8]; // Funct3
    private readonly delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[] _branchTable = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[8]; // Funct3
    private readonly RiscVCpu<T> _processor;
    private readonly T* _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionServiceTable{T, TSigned}"/> struct.
    /// </summary>
    /// <param name="processor"></param>
    public RiscVInstructionServiceTable(RiscVCpu<T> processor)
    {
        _processor = processor;
        _regs = processor.RegisterFile.Regs;

        InitTables(processor.Config);
    }

    /// <inheritdoc/>
    public RiscVTrap Execute(RiscVInstruction instruction, out RiscVExecution<T> execution)
        => _opCodeTable[(int)instruction.OpCode](this, instruction, out execution);

    private static RiscVTrap DipatchLoad(RiscVInstructionServiceTable<T, TSigned> @this, RiscVInstruction instruction, out RiscVExecution<T> execution)
        => @this._loadTable[(int)instruction.OpCode](@this, instruction, out execution);

    private static RiscVTrap DipatchStore(RiscVInstructionServiceTable<T, TSigned> @this, RiscVInstruction instruction, out RiscVExecution<T> execution)
        => @this._storeTable[(int)instruction.OpCode](@this, instruction, out execution);

    private static RiscVTrap DipatchAluImmediate(RiscVInstructionServiceTable<T, TSigned> @this, RiscVInstruction instruction, out RiscVExecution<T> execution)
        => @this._aluImmTable[(int)instruction.Funct3](@this, instruction, out execution);

    private static RiscVTrap DipatchAluRegister(RiscVInstructionServiceTable<T, TSigned> @this, RiscVInstruction instruction, out RiscVExecution<T> execution)
        => @this._aluRegTable[(int)instruction.Funct3](@this, instruction, out execution);

    private static RiscVTrap Illegal(RiscVInstructionServiceTable<T, TSigned> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
    {
        exec = default;
        return RiscVTrap.IllegalInstruction;
    }

    private static RiscVTrap NotImplemented(RiscVInstructionServiceTable<T, TSigned> @this, RiscVInstruction inst, out RiscVExecution<T> exec)
        => throw new UnimplementedInstructionException(ulong.CreateTruncating(@this._processor.ProgramCounter));
}
