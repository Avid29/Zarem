// Avishai Dernis 2026

using System;
using System.Runtime.CompilerServices;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Models;

namespace Zarem.Mips.Models;

/// <summary>
/// An <see cref="InstructionDecodeTable{T, TInstruction}"/> for the MIPS architecture.
/// </summary>
public class MipsInstructionDecodeTable<T> : InstructionDecodeTable<T, MipsInstruction>
{
    private readonly T[] _opTable = new T[64];
    private readonly T[] _specialTable = new T[64];
    private readonly T[] _special2Table = new T[64];
    private readonly T[] _regImmTable = new T[32];

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionDecodeTable{T}"/> class.
    /// </summary>
    public MipsInstructionDecodeTable(T reserved)
    {
        Array.Fill(_opTable, reserved);
        Array.Fill(_specialTable, reserved);
        Array.Fill(_special2Table, reserved);
        Array.Fill(_regImmTable, reserved);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T Lookup(MipsInstruction instruction)
    {
        var op = instruction.OpCode; // Primary OpCode (bits 31-26)

        // Nested Switch/Table Dispatch
        return op switch
        {
            MipsOpCode.Special => _specialTable[(int)instruction.FuncCode],             // SPECIAL
            MipsOpCode.Special2 => _special2Table[(int)instruction.Func2Code],          // SPECIAL2
            MipsOpCode.RegisterImmediate => _regImmTable[(int)instruction.RTFuncCode],  // REGIMM
            _ => _opTable[(int)op]                                                      // Standard
        };
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
    public void Register(RegImmFuncCode funcCode, T value) => _regImmTable[(int)funcCode] = value;
}
