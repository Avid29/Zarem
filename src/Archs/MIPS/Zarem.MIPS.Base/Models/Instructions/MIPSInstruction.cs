// Avishai Dernis 2025

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Zarem.Helpers;
using Zarem.Helpers.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;
using Zarem.Services;

namespace Zarem.Models.Instructions;

//                               MIPS Primary Instructions Layout
// ----------------------------------------------------------------------------
//      All instructions in MIPS are 4-bytes (32 bits).
//
// There are 3 primary types of instructions.
// - R type
// - I type
// - J type.
// 
//                            R Type Instructions Summary
// ----------------------------------------------------------------------------
//      R type instructions split the field into an Operation Code (6 bits),
// RS register (5 bits), RT register (5 bits), RD register (5 bits),
// Shift Amount (5 bits), and Function Code (6 bits).
//
//
//                           R Type Instruction Components
// ----------------------------------------------------------------------------
//
// Operation Code:
//      R type instructions have an OP code of 0, and the function code is instead
//      used to differentiate instructions.
// 
// RS Register:
//      R type instructions *usually* use RS as the first input register argument,
//      immediate shift instructions being the exception.
// 
// RT Register:
//      R type instructions *usually* use RT as the second input register argument,
//      immediate shift instruction again being the exception.
//
// RD Register:
//      RD is the writeback register of the value calculated by the instruction.
//
// Shift Amount:
//      Shift amount is only used for immediate shift instructions. It specifies the
//      number of bits (unsigned) to shift in a given direction.
// 
// Function Code:
//      The function code is used to differentiate R type instructions.
//
//                       R Type Instruction Assembled Examples
// ----------------------------------------------------------------------------
// > add $t0, $s0, $s1
//         |  Oper  |  $rs  |  $rt  |  $rd  | Shift |  Func  |
//  ------ + ------ + ----- + ----- + ----- + ----- + ------ |
// Binary  | 000000 | 10000 | 10001 | 01000 | 00000 | 100000 |
// Hex     |     00 |    10 |    11 |    08 |    00 |     20 |
// Decimal |      0 |    16 |    17 |     8 |     0 |     32 |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Meaning | R Type |   $s0 |   $s1 |   $t0 |   N/A |    add |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Binary  |    00000010 00010001 01000000 00100000 |
// Hex     |                            02 11 40 20 |
// ------- + -------------------------------------- +
//
// > sll $t0, $s0, 3
//         |  Oper  |  $rs  |  $rt  |  $rd  | Shift |  Func  |
//  ------ + ------ + ----- + ----- + ----- + ----- + ------ |
// Binary  | 000000 | 00000 | 10000 | 01000 | 00011 | 000000 |
// Hex     |     00 |    00 |    10 |    08 |    03 |     00 |
// Decimal |      0 |     0 |    16 |     8 |     3 |      0 |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Meaning | R Type |   N/A |   $s0 |   $t0 |     3 |    sll |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Binary  |    00000000 00010000 01000000 11000000 |
// Hex     |                            00 10 40 c0 |
// ------- + -------------------------------------- +
//
// 
//                            I Type Instructions Summary
// ----------------------------------------------------------------------------
//      I type instructions split the field into an Operation Code (6 bits),
// RS register (5 bits), RT register (5 bits), and an immediate value (16 bits).
//
//
//                           I Type Instruction Components
// ----------------------------------------------------------------------------
//
// Operation Code:
//      The OP code is used to identify the instruction to run.
// 
// RS Register:
//      I type instructions use RS as the first input register argument if required.
// 
// RT Register:
//      I type instructions use RT as the write back register, except in memory saving
//      and in branch comparing where two argument registers are required.

// Immediate Value:
//      The function code is used to differentiate R type instructions.
//
//                       I Type Instruction Assembled Examples
// ----------------------------------------------------------------------------
// > addi $t0, $s0, 20
//         |  Oper  |  $rs  |  $rt  | Immediate Value  |
//  ------ + ------ + ----- + ----- + ---------------- |
// Binary  | 001000 | 10000 | 01000 | 0000000000010100 |
// Hex     |     08 |    10 |    08 |               14 |
// Decimal |      8 |    16 |     8 |               20 |
// ------- + ------ + ----- + ----- + ---------------- +
// Meaning |   addi |   $s0 |   $t0 |               20 |
// ------- + ------ + ----- + ----- + ------------ + - +
// Binary  |   00100010 00001000 00000000 00010100 |
// Hex     |                           22 08 00 14 |
// ------- + ------------------------------------- +
//
// > bltz $a0, $t0, (label of +64)
//         |  Oper  |  $rs  |  $rt  | Immediate Value  |
//  ------ + ------ + ----- + ----- + ---------------- |
// Binary  | 000110 | 00100 | 01000 | 0000000000010000 |
// Hex     |     06 |    04 |    08 |               10 |
// Decimal |      6 |     4 |     8 |               16 |
// ------- + ------ + ----- + ----- + ---------------- +
// Meaning |   bltz |   $a0 |   $t0 |               64 |
// ------- + ------ + ----- + ----- + ------------ + - +
// Binary  |   00011000 10001000 00000000 00010000 |
// Hex     |                           18 88 00 10 |
// ------- + ------------------------------------- +
//
// Note: The meaning of the immediate value is 4x the actual value because the instructions are aligned
// to the 4 byte boundary. As a result, the last 4 bits can be dropped and can be utilized in the front
// to represent an effectively 20 bit offset.
//
//
// > sw $s0, 24($sp)
//         |  Oper  |  $rs  |  $rt  | Immediate Value  |
//  ------ + ------ + ----- + ----- + ---------------- |
// Binary  | 101011 | 11101 | 10000 | 0000000000011000 |
// Hex     |     2b |    1D |    10 |               18 |
// Decimal |     43 |    29 |    16 |               24 |
// ------- + ------ + ----- + ----- + ---------------- +
// Meaning |     sw |   $sp |   $s0 |               24 |
// ------- + ------ + ----- + ----- + ------------ + - +
// Binary  |   10101111 10110000 00000000 00011000 |
// Hex     |                           af b0 00 18 |
// ------- + ------------------------------------- +
//
//
// 
//                            J Type Instructions Summary
// ----------------------------------------------------------------------------
//      I type instructions split the field into an Operation Code (6 bits),
//  and an address value (26 bits).
//
//                           J Type Instruction Components
// ----------------------------------------------------------------------------
//
// Operation Code:
//      The OP code is used to identify the instruction to run.

// Address:
//      The jump address. This address must be word aligned, so the last 2 bits are dropped, making
//      it effectively a 28 bit address. The remaining 4 bits are assumed to be equivalent to that
//      of the current program counter.
//
//                       J Type Instruction Assembled Examples
// ----------------------------------------------------------------------------
// > j (label at 9,912)
//         |  Oper  |              Address          |
//  ------ + ------ + ----------------------------- |
// Binary  | 000010 | 00 00000000 00001001 10101110 |
// Hex     |     02 |                         09 AE |
// Decimal |      2 |                          2478 |
// ------- + ------ + ----------------------------- +
// Meaning |      j |                          9912 |
// ------- + ------ + ----------------------------- +
// Binary  |    00001000 00000000 00001001 10101110 |
// Hex     |                            08 00 09 ae |
// ------- + -------------------------------------- +
//
// > jal (label at 6,808)
//         |  Oper  |              Address          |
//  ------ + ------ + ----------------------------- |
// Binary  | 000011 | 00 00000000 00000110 10100110 |
// Hex     |     03 |                         06 A6 |
// Decimal |      3 |                          1702 |
// ------- + ------ + ----------------------------- +
// Meaning |    jal |                          6808 |
// ------- + ------ + ----------------------------- +
// Binary  |    00001100 00000000 00000110 10100110 |
// Hex     |                            0c 00 06 a6 |
// ------- + -------------------------------------- +

/// <summary>
/// A struct representing an instruction.
/// </summary>
[DebuggerDisplay("{Disassembled}")]
public struct MipsInstruction
{
    // Universal
    private const int OPCODE_BIT_SIZE = 6;
    private const int REGISTER_BIT_SIZE = 5;
    private const int OPCODE_BIT_OFFSET = ADDRESS_BIT_SIZE;

    // R Type
    private const int SHIFT_AMOUNT_BIT_SIZE = 5;
    private const int FUNCTION_BIT_SIZE = 6;

    private const int RS_BIT_OFFSET = REGISTER_BIT_SIZE + RT_BIT_OFFSET;
    private const int RT_BIT_OFFSET = (REGISTER_BIT_SIZE + RD_BIT_OFFSET);
    private const int RD_BIT_OFFSET = (SHIFT_AMOUNT_BIT_SIZE + SHIFT_AMOUNT_BIT_OFFSET);

    private const int SHIFT_AMOUNT_BIT_OFFSET = (FUNCTION_BIT_SIZE + FUNCTION_BIT_OFFSET);
    private const int FUNCTION_BIT_OFFSET = 0;

    // I Type
    private const int IMMEDIATE_BIT_SIZE = 16;
    private const int IMMEDIATE_BIT_OFFSET = 0;

    // J Type
    private const int ADDRESS_BIT_SIZE = 26;
    private const int ADDRESS_BIT_OFFSET = 0;

    private uint _inst;

    /// <summary>
    /// Creates a new <see cref="InstructionType.BasicR"/> instruction.
    /// </summary>
    public static MipsInstruction Create(byte opCode, byte funcCode, GPRegister rs, GPRegister rt, GPRegister rd, byte shiftAmount = 0)
    {
        MipsInstruction value = default;
        value.OpCode = (OperationCode)opCode;
        value.RS = rs;
        value.RT = rt;
        value.RD = rd;
        value.ShiftAmount = shiftAmount;
        value.FuncCode = (FunctionCode)funcCode;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="InstructionType.BasicR"/> instruction.
    /// </summary>
    public static MipsInstruction Create(FunctionCode funcCode, GPRegister rs, GPRegister rt, GPRegister rd, byte shiftAmount = 0)
    {
        MipsInstruction value = default;
        value.OpCode = OperationCode.Special;
        value.RS = rs;
        value.RT = rt;
        value.RD = rd;
        value.ShiftAmount = shiftAmount;
        value.FuncCode = funcCode;
        return value;
    }
    
    /// <summary>
    /// Creates a new <see cref="InstructionType.Special2R"/> instruction.
    /// </summary>
    public static MipsInstruction Create(Func2Code func2Code, GPRegister rs, GPRegister rt, GPRegister rd, byte shiftAmount = 0)
    {
        MipsInstruction value = default;
        value.OpCode = OperationCode.Special2;
        value.RS = rs;
        value.RT = rt;
        value.RD = rd;
        value.ShiftAmount = shiftAmount;
        value.Func2Code = func2Code;
        return value;
    }
    
    /// <summary>
    /// Creates a new <see cref="InstructionType.Special3R"/> instruction.
    /// </summary>
    public static MipsInstruction Create(Func3Code func3Code, GPRegister rs, GPRegister rt, GPRegister rd, byte shiftAmount = 0)
    {
        MipsInstruction value = default;
        value.OpCode = OperationCode.Special3;
        value.RS = rs;
        value.RT = rt;
        value.RD = rd;
        value.ShiftAmount = shiftAmount;
        value.Func3Code = func3Code;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="InstructionType.BasicI"/> instruction.
    /// </summary>
    public static MipsInstruction Create(OperationCode opCode, GPRegister rs, GPRegister rt, short immediate)
    {
        MipsInstruction value = default;
        value.OpCode = opCode;
        value.RS = rs;
        value.RT = rt;
        value.ImmediateValue = immediate;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="InstructionType.BasicI"/> instruction.
    /// </summary>
    public static MipsInstruction Create(OperationCode opCode, GPRegister rs, GPRegister rt, int offset)
    {
        MipsInstruction value = default;
        value.OpCode = opCode;
        value.RS = rs;
        value.RT = rt;
        value.Offset = offset;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="InstructionType.BasicI"/> instruction.
    /// </summary>
    /// <remarks>
    /// This is just for load upper immediate.
    /// </remarks>
    public static MipsInstruction Create(OperationCode opCode, GPRegister rt, short immediate)
    {
        MipsInstruction value = default;
        value.OpCode = opCode;
        value.RT = rt;
        value.ImmediateValue = immediate;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="InstructionType.BasicJ"/> instruction.
    /// </summary>
    public static MipsInstruction Create(OperationCode opCode, uint address)
    {
        MipsInstruction value = default;
        value.OpCode = opCode;
        value.Address = address;
        return value;
    }
    
    /// <summary>
    /// Creates a new <see cref="InstructionType.RegisterImmediate"/> instruction.
    /// </summary>
    public static MipsInstruction Create(RegImmFuncCode code, GPRegister rs, short immediate)
    {
        MipsInstruction value = default;
        value.OpCode = OperationCode.RegisterImmediate;
        value.RTFuncCode = code;
        value.RS = rs;
        value.ImmediateValue = immediate;
        return value;
    }

    /// <summary>
    /// Creates a new <see cref="InstructionType.RegisterImmediateBranch"/> branch instruction.
    /// </summary>
    public static MipsInstruction Create(RegImmFuncCode code, GPRegister rs, int offset)
    {
        MipsInstruction value = default;
        value.OpCode = OperationCode.RegisterImmediate;
        value.RTFuncCode = code;
        value.RS = rs;
        value.Offset = offset;
        return value;
    }

    /// <summary>
    /// Gets a no operation instruction.
    /// </summary>
    public static MipsInstruction NOP => (MipsInstruction)0;

    /// <summary>
    /// Gets the instruction type.
    /// </summary>
    public readonly InstructionType Type => InstructionTypeHelper.GetInstructionType(OpCode, RTFuncCode, (CoProc0RSCode)RS);

    /// <summary>
    /// Gets the instruction's operation code.
    /// </summary>
    public OperationCode OpCode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (OperationCode)BitField.GetField(_inst, OPCODE_BIT_SIZE, OPCODE_BIT_OFFSET);
        set => BitField.SetField(ref _inst, OPCODE_BIT_SIZE, OPCODE_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's RS Register 
    /// </summary>
    public GPRegister RS
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (GPRegister)BitField.GetField(_inst, REGISTER_BIT_SIZE, RS_BIT_OFFSET);
        set => BitField.SetField(ref _inst, REGISTER_BIT_SIZE, RS_BIT_OFFSET, (uint)value);
    }
    
    /// <summary>
    /// Gets the instruction's RT Register 
    /// </summary>
    public GPRegister RT
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (GPRegister)BitField.GetField(_inst, REGISTER_BIT_SIZE, RT_BIT_OFFSET);
        set => BitField.SetField(ref _inst, REGISTER_BIT_SIZE, RT_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's RT Func code.
    /// </summary>
    /// <remarks>
    /// This is stored in the RT register space for an instruction where
    /// the <see cref="OpCode"/> is <see cref="OperationCode.RegisterImmediate"/>.
    /// </remarks>
    public RegImmFuncCode RTFuncCode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (RegImmFuncCode)RT;
        set => RT = (GPRegister)value;
    }

    /// <summary>
    /// Gets the instruction's RD Register 
    /// </summary>
    public GPRegister RD
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (GPRegister)BitField.GetField(_inst, REGISTER_BIT_SIZE, RD_BIT_OFFSET);
        set => BitField.SetField(ref _inst, REGISTER_BIT_SIZE, RD_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's shift amount 
    /// </summary>
    public byte ShiftAmount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (byte)BitField.GetField(_inst, SHIFT_AMOUNT_BIT_SIZE, SHIFT_AMOUNT_BIT_OFFSET);
        set => BitField.SetField(ref _inst, SHIFT_AMOUNT_BIT_SIZE, SHIFT_AMOUNT_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's function code.
    /// </summary>
    /// <remarks>
    /// Instruction may or may not have function code.
    /// </remarks>
    public FunctionCode FuncCode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (FunctionCode)BitField.GetField(_inst, FUNCTION_BIT_SIZE, FUNCTION_BIT_OFFSET);
        set => BitField.SetField(ref _inst, FUNCTION_BIT_SIZE, FUNCTION_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the instruction's function2 code.
    /// </summary>
    /// <remarks>
    /// Instruction may or may not have function2 code.
    /// </remarks>
    public Func2Code Func2Code
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (Func2Code)FuncCode;
        set => FuncCode = (FunctionCode)value;
    }

    /// <summary>
    /// Gets the instruction's function3 code.
    /// </summary>
    /// <remarks>
    /// Instruction may or may not have function3 code.
    /// </remarks>
    public Func3Code Func3Code
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (Func3Code)FuncCode;
        set => FuncCode = (FunctionCode)value;
    }

    /// <summary>
    /// Gets the instruction's immediate value.
    /// </summary>
    public short ImmediateValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (short)BitField.GetField(_inst, IMMEDIATE_BIT_SIZE, IMMEDIATE_BIT_OFFSET);
        set => BitField.SetField(ref _inst, IMMEDIATE_BIT_SIZE, IMMEDIATE_BIT_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets the instructions offset value.
    /// </summary>
    public int Offset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => ImmediateValue << 2;
        set => ImmediateValue = (short)(value >> 2);
    }

    /// <summary>
    /// Gets the instruction's immediate value.
    /// </summary>
    public uint Address
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetField(_inst, ADDRESS_BIT_SIZE, ADDRESS_BIT_OFFSET) << 2;
        set => BitField.SetField(ref _inst, ADDRESS_BIT_SIZE, ADDRESS_BIT_OFFSET, value >> 2);
    }
    
    #if DEBUG

    /// <summary>
    /// Gets the instruction disassembled as assembly code.
    /// </summary>
    public readonly string Disassembled => ServiceCollection.DisassemblerService?.Disassemble(this) ?? "No disassembler provided";

    #endif

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="MipsInstruction"/>.
    /// </summary>
    public static explicit operator MipsInstruction(uint value) => Unsafe.As<uint, MipsInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="MipsInstruction"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(MipsInstruction value) => Unsafe.As<MipsInstruction, uint>(ref value);
}
