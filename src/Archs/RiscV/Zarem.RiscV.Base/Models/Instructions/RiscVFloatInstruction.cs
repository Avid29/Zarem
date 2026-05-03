// Avishai Dernis 2024

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Models.Instructions;

//                     MIPS Floating-Point Instructions Layout
// ----------------------------------------------------------------------------
//      Like all instructions in MIPS, floating-point instructions
//  are 4-bytes (32 bits).
//
//                      Floating-Point Instruction Summary
// ----------------------------------------------------------------------------
//      Floating-Point instructions split the field into an Operation Code
// (6 bits), Format (5 bits), FT register (5 bits), FS register (5 bits),
// FD register (5 bits), and Function Code (6 bits).
//
//                      Floating-Point Instruction Components
// ----------------------------------------------------------------------------
//
// Operation Code:
//      The operation code for floating-point instructions is 17 (0x11). This
//      is the coprocessor1 op-code.
//
// Format:
//      Floating-point instructions contain a format parameter, declaring the
//      format of the floating-point values.
//
// FT Register:
//      FT is the second input register argument. It is an FPU register index.
//
// FS Register:
//      FS is the first input register argument. It is an FPU register index.
//
// FD Register:
//      FD is the writeback register for the result of the calculation. It is
//      an FPU register index.
//
// Function code:
//      The function code is used to differentiate floating-type instructions.
//
//                 Floating-Point Instruction Assembled Examples
// ----------------------------------------------------------------------------
// > add.S $f25, $f5, $f18
//         |  Oper  |  fmt  |  $ft  |  $fs  |  $fd  |  Func  |
//  ------ + ------ + ----- + ----- + ----- + ----- + ------ |
// Binary  | 010001 | 10000 | 10010 | 00101 | 11001 | 000000 |
// Hex     |     11 |    10 |    12 |    05 |    19 |     00 |
// Decimal |     17 |    16 |    18 |     5 |    25 |      0 |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Meaning | CoPrc1 | Singl |  $f18 |   $f5 |  $f25 |    add |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Binary  |    01000110 00010010 00101110 01000000 |
// Hex     |                            46 12 2e 40 |
// ------- + -------------------------------------- +
//
// > cvt.W.D $f10, $f8
//         |  Oper  |  fmt  |  $ft  |  $fs  |  $fd  |  Func  |
//  ------ + ------ + ----- + ----- + ----- + ----- + ------ |
// Binary  | 010001 | 10001 | 00000 | 01000 | 00011 | 100100 |
// Hex     |     00 |    11 |    00 |    08 |    0a |     24 |
// Decimal |      0 |    17 |     0 |     8 |    10 |     36 |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Meaning | CoPrc1 | Doubl |   N/A |   $f8 |  $f10 |  cvt.W |
// ------- + ------ + ----- + ----- + ----- + ----- + ------ +
// Binary  |    01000110 00100000 01000000 11100100 |
// Hex     |                            46 20 40 e4 |
// ------- + -------------------------------------- +

/// <summary>
/// A struct representing an instruction utilizing the floating-point coprocessor.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct RiscVFloatInstruction
{
    // Opcodes are 7 bits, Registers are 5 bits.
    private const int OPCODE_BIT_SIZE = 7;
    private const int REG_BIT_SIZE = 5;
    private const int FMT_BIT_SIZE = 2;
    private const int FUNCT_BIT_SIZE = 5;

    private const int OPCODE_OFFSET = 0;
    private const int RD_OFFSET = 7;
    private const int FUNCT3_OFFSET = 12;
    private const int RS1_OFFSET = 15;
    private const int RS2_OFFSET = 20;
    private const int FUNCT_OFFSET = 25;
    private const int FMT_OFFSET = 25;
    private const int RS3_OFFSET = 27;

    [FieldOffset(0)]
    private uint _inst;

    /// <summary>
    /// Gets or sets the instruction's operation code.
    /// </summary>
    public RiscVOpCode OpCode
    {
        readonly get => (RiscVOpCode)BitField.GetField(_inst, OPCODE_BIT_SIZE, OPCODE_OFFSET);
        set => BitField.SetField(ref _inst, OPCODE_BIT_SIZE, OPCODE_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's format.
    /// </summary>
    public RiscVFloatFormat Format
    {
        readonly get => (RiscVFloatFormat)BitField.GetField(_inst, FMT_BIT_SIZE, FMT_OFFSET);
        set => BitField.SetField(ref _inst, FMT_BIT_SIZE, FMT_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RD register.
    /// </summary>
    public RiscVFloatRegister RD
    {
        readonly get => (RiscVFloatRegister)BitField.GetField(_inst, REG_BIT_SIZE, RD_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RD_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RS1 register.
    /// </summary>
    public RiscVFloatRegister RS1
    {
        readonly get => (RiscVFloatRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS1_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS1_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RS2 register.
    /// </summary>
    public RiscVFloatRegister RS2
    {
        readonly get => (RiscVFloatRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS2_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS2_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RS2 register.
    /// </summary>
    public RiscVFloatRegister RS3
    {
        readonly get => (RiscVFloatRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS3_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS3_OFFSET, (byte)value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="RiscVFloatInstruction"/>.
    /// </summary>
    public static explicit operator RiscVFloatInstruction(uint value) => Unsafe.As<uint, RiscVFloatInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="RiscVFloatInstruction"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(RiscVFloatInstruction value) => Unsafe.As<RiscVFloatInstruction, uint>(ref value);

    /// <summary>
    /// Casts an <see cref="RiscVInstruction"/> to a <see cref="RiscVFloatInstruction"/>.
    /// </summary>
    public static implicit operator RiscVFloatInstruction(RiscVInstruction value) => Unsafe.As<RiscVInstruction, RiscVFloatInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="RiscVFloatInstruction"/> to a <see cref="RiscVInstruction"/>.
    /// </summary>
    public static implicit operator RiscVInstruction(RiscVFloatInstruction value) => Unsafe.As<RiscVFloatInstruction, RiscVInstruction>(ref value);
}
