// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Runtime.CompilerServices;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;
using Zarem.Mips.Models.Instructions.Enums.Operations;

namespace Zarem.Mips.Models;

/// <summary>
/// A class for looking 
/// </summary>
/// <typeparam name="T"></typeparam>
public class MipsInstructionDecodeTable<T>
{
    private readonly T[] _opTable = new T[64];
    private readonly T[] _specialTable = new T[64];
    private readonly T[] _special2Table = new T[64];
    private readonly T[] _special3Table = new T[64];
    private readonly T[] _regImmTable = new T[32];

    // Coprocessor1
    private readonly T[] _coProc0Table = new T[32];
    private readonly T[] _coProc0FuncTable = new T[64];
    private readonly T[] _coProcMfmc0FuncTable = new T[64];
    private readonly T[] _coProc1Table = new T[32];
    private readonly T[] _floatTable = new T[4 * 64];   // Float: 4 formats (S, D, W, L) * 64 func codes

    /// <summary>
    /// Initializes a new instance of the MipsInstructionTable class with the specified value representing an invalid
    /// instruction.
    /// </summary>
    /// <param name="reserved">The value to use for invalid or unrecognized instructions. This value is returned when a lookup fails to match a valid instruction.</param>
    public MipsInstructionDecodeTable(T reserved)
    {
        Array.Fill(_opTable, reserved);
        Array.Fill(_specialTable, reserved);
        Array.Fill(_special2Table, reserved);
        Array.Fill(_regImmTable, reserved);
        Array.Fill(_coProc0Table, reserved);
        Array.Fill(_coProc0FuncTable, reserved);
        Array.Fill(_coProcMfmc0FuncTable, reserved);
        Array.Fill(_coProc1Table, reserved);
        Array.Fill(_floatTable, reserved);
    }

    /// <summary>
    /// Looks up an instruction
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Lookup(MipsInstruction instruction)
    {
        var op = instruction.OpCode; // Primary OpCode (bits 31-26)

        // Nested Switch/Table Dispatch
        return op switch
        {
            MipsOpCode.Special => _specialTable[(int)instruction.FuncCode],                 // SPECIAL
            MipsOpCode.Special2 => _special2Table[(int)instruction.Func2Code],              // SPECIAL2
            MipsOpCode.Special3 => _special3Table[(int)instruction.Func3Code],              // SPECIAL2
            MipsOpCode.RegisterImmediate => _regImmTable[(int)instruction.RTFuncCode],      // REGIMM
            MipsOpCode.Coprocessor0 => LookupCoProc0(instruction),                          // COP0
            MipsOpCode.Coprocessor1 => LookupFloat(instruction),                            // COP1
            _ => _opTable[(int)op]                                                          // Standard
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T LookupCoProc0(CoProc0Instruction instruction)
    {
        var rsCode = instruction.CoProc0RSCode;
        return rsCode switch
        {
            CoProc0RSCode.C0 => _coProc0FuncTable[(int)instruction.Co0FuncCode],
            CoProc0RSCode.MFMC0 => _coProcMfmc0FuncTable[(int)instruction.MFMC0FuncCode],
            _ => _coProc0Table[(int)rsCode],
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T LookupFloat(MipsFloatInstruction instruction)
    {
        var rsCode = instruction.RSCode;
        switch (rsCode)
        {
            case CoProc1RSCode.Single:
            case CoProc1RSCode.Double:
            case CoProc1RSCode.Word:
            case CoProc1RSCode.Long:
                int fmt = GetFloatFormatIndex(instruction.Format);
                return _floatTable[(fmt << 6) | (int)instruction.Function];
            default:
                return _coProc1Table[(int)rsCode];
        }

    }

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(MipsOpCode opCode, T value) => _opTable[(int)opCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(FunctionCode funcCode, T value) => _specialTable[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(Func2Code funcCode, T value) => _special2Table[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(Func3Code funcCode, T value) => _special3Table[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(RegImmFuncCode funcCode, T value) => _regImmTable[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(CoProc0RSCode funcCode, T value) => _coProc0Table[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(Co0FuncCode funcCode, T value) => _coProc0FuncTable[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(MFMC0FuncCode funcCode, T value) => _coProcMfmc0FuncTable[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(CoProc1RSCode funcCode, T value) => _coProc1Table[(int)funcCode] = value;

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(MipsFloatFuncCode funcCode, T value)
    {
        Register(MipsFloatFormat.Single, funcCode, value);
        Register(MipsFloatFormat.Double, funcCode, value);
        Register(MipsFloatFormat.Word, funcCode, value);
        Register(MipsFloatFormat.Long, funcCode, value);
    }

    /// <summary>
    /// Registers an instruction.
    /// </summary>
    public void Register(MipsFloatFormat format, MipsFloatFuncCode funcCode, T value)
    {
        int fmt = GetFloatFormatIndex(format);
        _floatTable[(fmt << 6) | (int)funcCode] = value;
    }

    /// <summary>
    /// Gets the <see cref="MipsFloatFormat"/> of a given primitive
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MipsFloatFormat GetFloatFuncTableIndex<TFormat>()
        where TFormat : unmanaged
    {
        if (typeof(TFormat) == typeof(float)) return MipsFloatFormat.Single;
        if (typeof(TFormat) == typeof(double)) return MipsFloatFormat.Double;
        if (typeof(TFormat) == typeof(int)) return MipsFloatFormat.Word;
        if (typeof(TFormat) == typeof(long)) return MipsFloatFormat.Long;
        else return ThrowHelper.ThrowFormatException<MipsFloatFormat>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFloatFormatIndex(MipsFloatFormat format)
    {
        return format switch
        {
            MipsFloatFormat.Single => 0,
            MipsFloatFormat.Double => 1,
            MipsFloatFormat.Word => 2,
            MipsFloatFormat.Long => 3,
            _ => ThrowHelper.ThrowFormatException<int>(),
        };
    }
}
