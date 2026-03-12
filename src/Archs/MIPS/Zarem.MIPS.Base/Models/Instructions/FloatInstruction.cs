// Avishai Dernis 2024

using System.Runtime.CompilerServices;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

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
public struct FloatInstruction
{
    private MipsInstruction _inst;
    
    /// <summary>
    /// Creates a new floating-point coprocessor instruction.
    /// </summary>
    public static FloatInstruction Create(FloatFuncCode funcCode, FloatFormat format, FloatRegister fs, FloatRegister fd, FloatRegister ft = FloatRegister.F0)
    {
        FloatInstruction value = default;
        value.OpCode = OperationCode.Coprocessor1;
        value.FloatFuncCode = funcCode;
        value.Format = format;
        value.FS = fs;
        value.FD = fd;
        value.FT = ft;
        return value;
    }
    
    /// <summary>
    /// Creates a new floating-point coprocessor instruction.
    /// </summary>
    public static FloatInstruction Create(CoProc1RSCode code, GPRegister rt, FloatRegister fs)
    {
        FloatInstruction value = default;
        value.OpCode = OperationCode.Coprocessor1;
        value.CoProc1RSCode = code;
        value.RT = rt;
        value.FS = fs;
        return value;
    }

    /// <summary>
    /// Gets the instruction's operation code.
    /// </summary>
    public OperationCode OpCode
    { 
        readonly get => _inst.OpCode;
        private set => _inst.OpCode = value;
    }

    /// <summary>
    /// Gets the instruction's float function code.
    /// </summary>
    public FloatFuncCode FloatFuncCode
    {
        readonly get => (FloatFuncCode)_inst.FuncCode;
        private set => _inst.FuncCode = (FunctionCode)value;
    }

    /// <summary>
    /// Gets the instruction's format.
    /// </summary>
    public CoProc1RSCode CoProc1RSCode
    {
        readonly get => (CoProc1RSCode)_inst.RS;
        private set => _inst.RS = (GPRegister)value;
    }

    /// <summary>
    /// Gets the instruction's format.
    /// </summary>
    public FloatFormat Format
    {
        readonly get => (FloatFormat)_inst.RS;
        private set => _inst.RS = (GPRegister)value;
    }

    /// <summary>
    /// Gets the instruction's FT Register.
    /// </summary>
    public FloatRegister FT
    {
        readonly get => (FloatRegister)_inst.RT;
        private set => _inst.RT = (GPRegister)value;
    }

    /// <summary>
    /// Gets the instruction's RT Register.
    /// </summary>
    public GPRegister RT
    {
        readonly get => _inst.RT;
        private set => _inst.RT = value;
    }

    /// <summary>
    /// Gets the instruction's FS Register.
    /// </summary>
    public FloatRegister FS
    {
        readonly get => (FloatRegister)_inst.RD;
        private set => _inst.RD = (GPRegister)value;
    }

    /// <summary>
    /// Gets the instruction's FD Register.
    /// </summary>
    public FloatRegister FD
    {
        readonly get => (FloatRegister)_inst.ShiftAmount;
        private set => _inst.ShiftAmount = (byte)value;
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="FloatInstruction"/>.
    /// </summary>
    public static unsafe explicit operator FloatInstruction(uint value) => Unsafe.As<uint, FloatInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="FloatInstruction"/> to a <see cref="uint"/>.
    /// </summary>
    public static unsafe explicit operator uint(FloatInstruction value) => Unsafe.As<FloatInstruction, uint>(ref value);

    /// <summary>
    /// Casts an <see cref="MipsInstruction"/> to a <see cref="FloatInstruction"/>.
    /// </summary>
    public static unsafe implicit operator FloatInstruction(MipsInstruction value) => Unsafe.As<MipsInstruction, FloatInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="FloatInstruction"/> to a <see cref="MipsInstruction"/>.
    /// </summary>
    public static unsafe implicit operator MipsInstruction(FloatInstruction value) => Unsafe.As<FloatInstruction, MipsInstruction>(ref value);
}
