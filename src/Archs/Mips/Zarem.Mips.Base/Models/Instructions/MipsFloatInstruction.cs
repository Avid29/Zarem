// Avishai Dernis 2024

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Models.Instructions;

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
public struct MipsFloatInstruction
{
    [FieldOffset(0)]
    private MipsInstruction _inst;

    /// <summary>
    /// Creates a new floating-point coprocessor instruction.
    /// </summary>
    public static MipsFloatInstruction Create(MipsFloatFuncCode funcCode, MipsFloatFormat format, MipsFloatRegister fs, MipsFloatRegister fd, MipsFloatRegister ft = MipsFloatRegister.F0)
    {
        return new()
        {
            OpCode = MipsOpCode.Coprocessor1,
            Function = funcCode,
            Format = format,
            FS = fs,
            FD = fd,
            FT = ft,
        };
    }

    /// <summary>
    /// Creates a new floating-point coprocessor instruction.
    /// </summary>
    public static MipsFloatInstruction Create(CoProc1RSCode code, MipsGpRegister rt, MipsFloatRegister fs)
    {
        return new()
        {
            OpCode = MipsOpCode.Coprocessor1,
            RSCode = code,
            RT = rt,
            FS = fs,
        };
    }

    /// <summary>
    /// Gets the instruction's operation code.
    /// </summary>
    public MipsOpCode OpCode
    {
        readonly get => _inst.OpCode;
        private set => _inst.OpCode = value;
    }

    /// <summary>
    /// Gets the instruction's function code.
    /// </summary>
    public MipsFloatFuncCode Function
    {
        readonly get => (MipsFloatFuncCode)_inst.FuncCode;
        private set => _inst.FuncCode = (FunctionCode)value;
    }

    /// <summary>
    /// Gets the instruction's RS Code.
    /// </summary>
    public CoProc1RSCode RSCode
    {
        readonly get => (CoProc1RSCode)_inst.RS;
        private set => _inst.RS = (MipsGpRegister)value;
    }

    /// <summary>
    /// Gets the instruction's format.
    /// </summary>
    public MipsFloatFormat Format
    {
        readonly get => (MipsFloatFormat)_inst.RS;
        private set => _inst.RS = (MipsGpRegister)value;
    }

    /// <summary>
    /// Gets the instruction's FT Register.
    /// </summary>
    public MipsFloatRegister FT
    {
        readonly get => (MipsFloatRegister)_inst.RT;
        private set => _inst.RT = (MipsGpRegister)value;
    }

    /// <summary>
    /// Gets the instruction's RT Register.
    /// </summary>
    public MipsGpRegister RT
    {
        readonly get => _inst.RT;
        private set => _inst.RT = value;
    }

    /// <summary>
    /// Gets the instruction's FS Register.
    /// </summary>
    public MipsFloatRegister FS
    {
        readonly get => (MipsFloatRegister)_inst.RD;
        private set => _inst.RD = (MipsGpRegister)value;
    }

    /// <summary>
    /// Gets the instruction's FD Register.
    /// </summary>
    public MipsFloatRegister FD
    {
        readonly get => (MipsFloatRegister)_inst.ShiftAmount;
        private set => _inst.ShiftAmount = (byte)value;
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="MipsFloatInstruction"/>.
    /// </summary>
    public static explicit operator MipsFloatInstruction(uint value) => Unsafe.As<uint, MipsFloatInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="MipsFloatInstruction"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(MipsFloatInstruction value) => Unsafe.As<MipsFloatInstruction, uint>(ref value);

    /// <summary>
    /// Casts an <see cref="MipsInstruction"/> to a <see cref="MipsFloatInstruction"/>.
    /// </summary>
    public static implicit operator MipsFloatInstruction(MipsInstruction value) => Unsafe.As<MipsInstruction, MipsFloatInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="MipsFloatInstruction"/> to a <see cref="MipsInstruction"/>.
    /// </summary>
    public static implicit operator MipsInstruction(MipsFloatInstruction value) => Unsafe.As<MipsFloatInstruction, MipsInstruction>(ref value);
}
