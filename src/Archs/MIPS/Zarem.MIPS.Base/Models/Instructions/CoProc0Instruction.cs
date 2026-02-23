// Avishai Dernis 2025

using System.Runtime.CompilerServices;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Models.Instructions;

//                     MIPS Co-Processor0 Instructions Layout
// ----------------------------------------------------------------------------
//      Like all instructions in MIPS, coprocessor0 instructions
//  are 4-bytes (32 bits).
//
//      CoProcessor0 instructions use the RS space as a function code.
//
//                      Co-Processor0 Instruction Summary
// ----------------------------------------------------------------------------
//      Co-Processor0 instructions split the field into an Operation Code
// (6 bits), RS Code (5 bits), and the remaining 21 bits are instruction dependent. 
//
//                      Co-Processor0 Instruction Components
// ----------------------------------------------------------------------------
//
// Operation Code:
//      The operation code for any Co-Processor0 instruction is 16 (0x10).
//
// RS Code:
//      The RS Code determines the type of c0 instruction. In many instances
//      it is a function code in-and-of itself.
//
// [Instruction Dependent Region]:
//      The usage of the 21 bits following the RS Code are dependent on
//      the c0 instruction.
//
//               Co-Processor0 Instruction Assembled Examples
// ----------------------------------------------------------------------------
// > mtc0 $t0, $16
//         |  Oper  | RSCod |  $rt  |  $rd  |          | Sel |
//  ------ + ------ + ------------- + ----- + -------- + ---- +
// Binary  | 010000 | 00100 | 00000 | 00000 | 00000000 | 000 |
// Hex     |     10 |    04 |       |       |       00 |  00 |
// Decimal |     16 |     4 |       |       |        0 |  00 |
// ------- + ------ + ----- + ----- + ----- + -------- + --- +
// Meaning | CoPrc0 | MovTo |       |       |      N/A |   0 |
// ------- + ------ + ----- + -------------------- + - + --- +
// Binary  |   01000000 10000000 00000000 00000000 |
// Hex     |                           40 80 00 00 |
// ------- + ------------------------------------- +
//
//                      Co-Processor0 C0 Instruction Summary
// ----------------------------------------------------------------------------
//      Co-Processor0 C0 instructions split the field into an Operation Code
// (6 bits), C0 bit (1 bit), and Function Code (6 bits).
//
//                      Co-Processor0 C0 Instruction Components
// ----------------------------------------------------------------------------
//
// Operation Code:
//      The operation code for any Co-Processor0 instruction is 16 (0x10).
//
// C0:
//      The C0 bit fills the upper-most bit of the RSCode range for the
//      Co-Processor0 instruction format. It is always 1.
//
// [Instruction Dependent Region]:
//      The usage of the 19 bits between the C0 bit and function code are
//      dependent on the c0 instruction.
//
// Function code:
//      The function code is used to differentiate Co-Processor0 C0
//      instructions.
//
//               Co-Processor0 C0 Instruction Assembled Examples
// ----------------------------------------------------------------------------
// > wait
//         |  Oper  | C0 |                     |  Func  |
//  ------ + ------ + ------------------------ + ------ |
// Binary  | 010000 |  1 | 0000000000000000000 | 100000 |
// Hex     |     10 |  1 |                  00 |     20 |
// Decimal |     16 |  1 |                   0 |     32 |
// ------- + ------ + -- + ------------------- + ------ +
// Meaning | CoPrc0 | C0 |                 N/A |   wait |
// ------- + ------ + -- + ------------------- + ------ +
// Binary  |    01000010 00000000 00000000 00100000 |
// Hex     |                            42 00 00 20 |
// ------- + -------------------------------------- +
//
//                      Co-Processor0 MFMC0 Instruction Summary
// ----------------------------------------------------------------------------
//      Co-Processor0 MFMC0 instructions split the field into an Operation Code
// (6 bits), RS Code (5 bits), RT register (5 bits), select code (1 bit), and Function Code (3 bits).
//
//                      Co-Processor0 MFMC0 Instruction Components
// ----------------------------------------------------------------------------
//
// Operation Code:
//      The operation code for any Co-Processor0 instruction is 16 (0x10).
//
// RS Code:
//      The RS Code for MFMC0 instructions is 11 (0xb).
//
// RT Register:
//      The RT register is the argument input or writeback register for
//      the instruction as needed.
//
// [Instruction Dependent Region]:
//      The usage of the 10 bits between the RS Code and SC bit are dependent
//      on the c0 instruction.
//
// SC Bit:
//      Bit indicating enabling or disabling for a set status instruction.
//
// [Reserved Region]:
//      There's two reserved bits before the function code.
//
// Function code:
//      The function code is used to differentiate Co-Processor0 MFMC0
//      instructions.
//
//               Co-Processor0 MFMC0 Instruction Assembled Examples
// ----------------------------------------------------------------------------
// > ei $t0
//         |  Oper  | MFMC0 |  $rt  |         |       | sc |    | Fnc |
//  ------ + ------ + ------------- + ------- + ----- + -- + -- + --- +
// Binary  | 010000 | 01011 | 01000 |   01100 | 00000 |  1 | 00 | 000 |
// Hex     |     10 |    0b |    08 |      0c |    00 | 01 | 00 | 000 |
// Decimal |     16 |    11 |     8 |      12 |     0 |  1 | 00 | 000 |
// ------- + ------ + ----- + ----- + ------- + ----- + -- + -- + --- +
// Meaning | CoPrc0 | MFMC0 |   $t0 | $status |   N/A | (↑enable)  ei |
// ------- + ------ + ----- + ----- + ------- + ----- + ------------- +
// Binary  |      01000001 01101000 01100000 00100000 |
// Hex     |                              41 68 60 20 |
// ------- + ---------------------------------------- +

/// <summary>
/// A struct representing an instruction utilizing coprocessor0.
/// </summary>
public partial struct CoProc0Instruction
{
    private MIPSInstruction _inst;

    /// <summary>
    /// Creates a new <see cref="OperationCode.Coprocessor0"/> instruction.
    /// </summary>
    public static CoProc0Instruction Create(CoProc0RSCode code, GPRegister rt, GPRegister rd)
    {
        CoProc0Instruction value = default;
        value.OpCode = OperationCode.Coprocessor0;
        value.CoProc0RSCode = code;
        value.RT = rt;
        value.RD = rd;

        return value;
    }
    
    /// <summary>
    /// Creates a new <see cref="CoProc0RSCode.C0"/> instruction.
    /// </summary>
    public static CoProc0Instruction Create(Co0FuncCode code, GPRegister rd)
    {
        CoProc0Instruction value = default;
        value.OpCode = OperationCode.Coprocessor0;
        value.Co0FuncCode = code;
        value.CoProc0RSCode = CoProc0RSCode.C0;
        value.RD = rd;

        return value;
    }
    
    /// <summary>
    /// Creates a new <see cref="CoProc0RSCode.MFMC0"/> instruction.
    /// </summary>
    public static CoProc0Instruction Create(MFMC0FuncCode code, GPRegister rt = GPRegister.Zero, byte? rd = null)
    {
        CoProc0Instruction value = default;
        value.OpCode = OperationCode.Coprocessor0;
        value.MFMC0FuncCode = code;
        value.CoProc0RSCode = CoProc0RSCode.MFMC0;
        value.RT = rt;

        // Conditionally assign
        value.RD = rd.HasValue ? (GPRegister)rd.Value : value.RD;

        return value;
    }

    /// <summary>
    /// Gets the instruction's operation code.
    /// </summary>
    public OperationCode OpCode
    { 
        readonly get => _inst.OpCode;
        internal set => _inst.OpCode = value;
    }

    /// <summary>
    /// Gets the instruction's RS function code.
    /// </summary>
    public CoProc0RSCode CoProc0RSCode
    { 
        readonly get => (CoProc0RSCode)_inst.RS;
        internal set => _inst.RS = (GPRegister)value;
    }

    /// <summary>
    /// Gets the instruction's RT Register 
    /// </summary>
    public GPRegister RT
    { 
        readonly get => _inst.RT;
        internal set => _inst.RT = value;
    }
    
    /// <summary>
    /// Gets the instruction's RD Register 
    /// </summary>
    public GPRegister RD
    { 
        readonly get => _inst.RD;
        internal set => _inst.RD = value;
    }
    
    /// <summary>
    /// Gets the instruction's coproc0 function code.
    /// </summary>
    /// <remarks>
    /// Instruction may or may not have coproc0 function code.
    /// </remarks>
    public Co0FuncCode Co0FuncCode
    { 
        readonly get => (Co0FuncCode)_inst.FuncCode;
        internal set => _inst.FuncCode = (FunctionCode)value;
    }

    /// <summary>
    /// Gets the instruction's mfmc0 function code.
    /// </summary>
    /// <remarks>
    /// Instruction may or may not have coproc0 function code.
    /// </remarks>
    public MFMC0FuncCode MFMC0FuncCode
    {
        readonly get => (MFMC0FuncCode)_inst.FuncCode;
        internal set => _inst.FuncCode = (FunctionCode)value;
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="CoProc0Instruction"/>.
    /// </summary>
    public static unsafe explicit operator CoProc0Instruction(uint value) => Unsafe.As<uint, CoProc0Instruction>(ref value);

    /// <summary>
    /// Casts a <see cref="CoProc0Instruction"/> to a <see cref="uint"/>.
    /// </summary>
    public static unsafe explicit operator uint(CoProc0Instruction value) => Unsafe.As<CoProc0Instruction, uint>(ref value);

    /// <summary>
    /// Casts an <see cref="MIPSInstruction"/> to a <see cref="CoProc0Instruction"/>.
    /// </summary>
    public static unsafe implicit operator CoProc0Instruction(MIPSInstruction value) => Unsafe.As<MIPSInstruction, CoProc0Instruction>(ref value);

    /// <summary>
    /// Casts a <see cref="CoProc0Instruction"/> to a <see cref="MIPSInstruction"/>.
    /// </summary>
    public static unsafe implicit operator MIPSInstruction(CoProc0Instruction value) => Unsafe.As<CoProc0Instruction, MIPSInstruction>(ref value);
}
