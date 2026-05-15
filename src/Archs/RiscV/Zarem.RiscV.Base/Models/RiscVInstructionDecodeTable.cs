// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Runtime.CompilerServices;
using Zarem.Models;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;

namespace Zarem.RiscV.Models;

/// <summary>
/// An <see cref="InstructionDecodeTable{T, TInstruction}"/> for the RISC-V architecture.
/// </summary>
public class RiscVInstructionDecodeTable<T> : InstructionDecodeTable<T, RiscVInstruction>
{
    private readonly T[][] _funct7Table = new T[128][];
    private readonly T[] _floatTable = new T[32 * 8 * 4];
    private readonly T[] _emptyTable = new T[128 * 8];

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionDecodeTable{T}"/> class.
    /// </summary>
    public RiscVInstructionDecodeTable(T illegal)
    {
        Array.Fill(_funct7Table, _emptyTable);
        Array.Fill(_floatTable, illegal);
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
        return instruction.OpCode switch
        {
            RiscVOpCode.Op or RiscVOpCode.Op32 or RiscVOpCode.Op64 => _funct7Table[(int)instruction.Funct7][GetLookupIndex(instruction)],
            RiscVOpCode.FloatCompute => _floatTable[GetLookupIndex((RiscVFloatInstruction)instruction)],
            _ => _funct7Table[(int)Funct7Code.Base][GetLookupIndex(instruction)],
        };
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
    public void Register(RiscVOpCode opCode, Funct3Code funct3, T value)
        => Register(Funct7Code.Base, opCode, funct3, value);

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(Funct7Code funct7, RiscVOpCode opCode, Funct3Code funct3, T value)
    {
        var table = _funct7Table[(int)funct7];
        if (table == _emptyTable)
        {
            table = new T[1024];
            Array.Fill(table, _emptyTable[0]);
            _funct7Table[(int)funct7] = table;
        }

        table[GetLookupIndex(opCode, funct3)] = value;
    }

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(RiscVFloatFormat format, FloatFunc5Code funct5, FloatFunct3Code funct3, T value)
    {
        _floatTable[GetLookupIndex(format, funct5, funct3)] = value;
    }

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(RiscVFloatFormat format, FloatFunc5Code funct5, T value)
    {
        _floatTable[GetLookupIndex(format, funct5, 0)] = value;
    }

    /// <summary>
    /// Gets the <see cref="RiscVFloatFormat"/> of a given primitive.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RiscVFloatFormat GetFloatFuncTableIndex<TFormat>()
        where TFormat : unmanaged
    {
        if (typeof(TFormat) == typeof(float)) return RiscVFloatFormat.Single;
        if (typeof(TFormat) == typeof(double)) return RiscVFloatFormat.Double;
        if (typeof(TFormat) == typeof(Half)) return RiscVFloatFormat.Half;
        // TODO: Quad
        else return ThrowHelper.ThrowFormatException<RiscVFloatFormat>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVInstruction instruction)
        => GetLookupIndex(instruction.OpCode, instruction.Funct3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVFloatInstruction instruction)
        => GetLookupIndex(instruction.Format, instruction.Funct5, instruction.Funct3);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVOpCode op, Funct3Code funct3)
        => (int)op << 3 | (int)funct3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLookupIndex(RiscVFloatFormat format, FloatFunc5Code funct5, FloatFunct3Code funct3)
        => (int)format << 8 | (int)funct5 << 3| (int)funct3;
}
