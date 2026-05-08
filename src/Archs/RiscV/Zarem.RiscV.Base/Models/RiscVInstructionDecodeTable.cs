// Avishai Dernis 2026

using System;
using System.Runtime.CompilerServices;
using Zarem.Models;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;

namespace Zarem.RiscV.Models;

/// <summary>
/// An <see cref="InstructionDecodeTable{T, TInstruction}"/> for the RISC-V architecture.
/// </summary>
public class RiscVInstructionDecodeTable<T> : InstructionDecodeTable<T, RiscVInstruction>
{
    private readonly T[][] _funct7Table = new T[128][];
    private readonly T[] _emptyTable = new T[1024];

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionDecodeTable{T}"/> class.
    /// </summary>
    public RiscVInstructionDecodeTable(T illegal)
    {
        Array.Fill(_funct7Table, _emptyTable);
        Array.Fill(_emptyTable, illegal);

        var @base = new T[1024];
        Array.Fill(@base, illegal);
        _funct7Table[(int)Funct7Code.Base] = @base;
        _funct7Table[(int)Funct7Code.Modified] = @base;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T Lookup(RiscVInstruction instruction)
    {
        var func7Code = instruction.OpCode switch
        {
            RiscVOpCode.Op or RiscVOpCode.Op32
            or RiscVOpCode.Op64 => instruction.Funct7,
            _ => Funct7Code.Base,
        };

        var table = _funct7Table[(int)func7Code];
        return table[GetLookupIndex(instruction)];
    }

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(RiscVOpCode opCode, T value)
    {
        for (int i = 0; i < 8; i++)
        {
            Register(opCode, (Funct3Code)i, value);
        }
    }

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(RiscVOpCode opCode, Funct3Code func3, T value)
        => Register(Funct7Code.Base, opCode, func3, value);

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(Funct7Code funct7, RiscVOpCode opCode, Funct3Code func3, T value)
    {
        var table = _funct7Table[(int)funct7];
        if (table == _emptyTable)
        {
            table = new T[1024];
            Array.Fill(table, _emptyTable[0]);
            _funct7Table[(int)funct7] = table;
        }

        table[GetLookupIndex(opCode, func3)] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVInstruction instruction)
        => GetLookupIndex(instruction.OpCode, instruction.Funct3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVOpCode op, Funct3Code funct3)
        => (int)op << 3 | (int)funct3;
}
