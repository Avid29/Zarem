// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Runtime.CompilerServices;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;
using Zarem.Models;

namespace Zarem.Mips.Models;

/// <summary>
/// An <see cref="InstructionDecodeTable{T, TInstruction}"/> for the MIPS floating-point architecture.
/// </summary>
public class MipsFloatInstructionDecodeTable<T> : InstructionDecodeTable<T, MipsFloatInstruction>
{
    // Coprocessor1
    private readonly T[] _coProc1Table = new T[32];
    private readonly T[] _floatTable = new T[4 * 64];   // Float: 4 formats (S, D, W, L) * 64 func codes

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsFloatInstructionDecodeTable{T}"/> class.
    /// </summary>
    public MipsFloatInstructionDecodeTable(T reserved)
    {
        Array.Fill(_floatTable, reserved);
    }
    
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T Lookup(MipsFloatInstruction instruction)
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
    public void Register(CoProc1RSCode funcCode, T value) => _coProc1Table[(int)funcCode] = value;

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
