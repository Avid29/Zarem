// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Models.Instructions;

/// <summary>
/// A struct representing a RISC-V instruction.
/// </summary>
public struct RiscVInstruction
{
    // Opcodes are 7 bits, Registers are 5 bits.
    private const int OPCODE_BIT_SIZE = 7;
    private const int REG_BIT_SIZE = 5;
    private const int FUNCT3_BIT_SIZE = 3;
    private const int FUNCT7_BIT_SIZE = 7;

    private const int OPCODE_OFFSET = 0;
    private const int RD_OFFSET = 7;
    private const int FUNCT3_OFFSET = 12;
    private const int RS1_OFFSET = 15;
    private const int RS2_OFFSET = 20;
    private const int FUNCT7_OFFSET = 25;

    private uint _inst;

    /// <summary>
    /// Creates an R-Type instruction.
    /// </summary>
    public static RiscVInstruction CreateR(RiscVOpCode op, Funct3Code f3, Funct7Code f7, RiscVGpRegister rd, RiscVGpRegister rs1, RiscVGpRegister rs2)
    {
        return new()
        {
            OpCode = op,
            Funct3 = f3,
            Funct7 = f7,
            RD = rd,
            RS1 = rs1,
            RS2 = rs2
        };
    }

    /// <summary>
    /// Creates an I-Type instruction.
    /// </summary>
    public static RiscVInstruction CreateI(RiscVOpCode op, Funct3Code f3, RiscVGpRegister rd, RiscVGpRegister rs1, short imm)
    {
        return new()
        {
            OpCode = op,
            Funct3 = f3,
            RD = rd,
            RS1 = rs1,
            Immediate = imm
        };
    }

    /// <summary>
    /// Creates an S-Type instruction (Store).
    /// </summary>
    public static RiscVInstruction CreateS(RiscVOpCode op, Funct3Code f3, RiscVGpRegister rs1, RiscVGpRegister rs2, short offset)
    {
        return new RiscVInstruction
        {
            OpCode = op,
            Funct3 = f3,
            RS1 = rs1,
            RS2 = rs2,
            StoreOffset = offset
        };
    }

    /// <summary>
    /// Creates a B-Type instruction (Branch).
    /// </summary>
    public static RiscVInstruction CreateB(RiscVOpCode op, Funct3Code f3, RiscVGpRegister rs1, RiscVGpRegister rs2, int offset)
    {
        return new()
        {
            OpCode = op,
            Funct3 = f3,
            RS1 = rs1,
            RS2 = rs2,
            BranchOffset = offset
        };
    }

    /// <summary>
    /// Creates a U-Type instruction (Upper Immediate).
    /// </summary>
    public static RiscVInstruction CreateU(RiscVOpCode op, RiscVGpRegister rd, int imm20)
    {
        return new()
        {
            OpCode = op,
            RD = rd,
            UpperImmediate = imm20
        };
    }

    /// <summary>
    /// Creates a J-Type instruction (Jump and Link).
    /// </summary>
    public static RiscVInstruction CreateJ(RiscVOpCode op, RiscVGpRegister rd, int offset)
    {
        return new()
        {
            OpCode = op,
            RD = rd,
            JumpOffset = offset
        };
    }

    /// <summary>
    /// Gets or sets the instruction's operation code.
    /// </summary>
    public RiscVOpCode OpCode
    {
        readonly get => (RiscVOpCode)BitField.GetField(_inst, OPCODE_BIT_SIZE, OPCODE_OFFSET);
        set => BitField.SetField(ref _inst, OPCODE_BIT_SIZE, OPCODE_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the destination general-purpose register field for this instruction.
    /// </summary>
    public RiscVGpRegister RD
    {
        readonly get => (RiscVGpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RD_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RD_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the value of the RS1 general-purpose register field.
    /// </summary>
    public RiscVGpRegister RS1
    {
        readonly get => (RiscVGpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS1_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS1_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the value of the RS2 general-purpose register field.
    /// </summary>
    public RiscVGpRegister RS2
    {
        readonly get => (RiscVGpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS2_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS2_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the value of the funct3 field.
    /// </summary>
    public Funct3Code Funct3
    {
        readonly get => (Funct3Code)BitField.GetField(_inst, FUNCT3_BIT_SIZE, FUNCT3_OFFSET);
        set => BitField.SetField(ref _inst, FUNCT3_BIT_SIZE, FUNCT3_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the value of the funct7 field.
    /// </summary>
    public Funct7Code Funct7
    {
        readonly get => (Funct7Code)BitField.GetField(_inst, FUNCT7_BIT_SIZE, FUNCT7_OFFSET);
        set => BitField.SetField(ref _inst, FUNCT7_BIT_SIZE, FUNCT7_OFFSET, (byte)value);
    }

    /// <summary>
    /// I-Type Immediates (12-bit, simple contiguous)
    /// Used for: addi, lw, jalr
    /// </summary>
    public short Immediate
    {
        readonly get => (short)((int)_inst >> 20); // Sign extended shift
        set => BitField.SetField(ref _inst, 12, 20, (uint)value & 0xFFF);
    }

    /// <summary>
    /// S-Type Immediates (12-bit, split across two fields)
    /// Used for: sw, sb, sh
    /// </summary>
    public short StoreOffset
    {
        readonly get
        {
            uint low = BitField.GetField(_inst, 5, 7);
            uint high = BitField.GetField(_inst, 7, 25);
            int val = (int)((high << 5) | low);
            return (short)((val << 20) >> 20); // Manual sign extension
        }
        set
        {
            BitField.SetField(ref _inst, 5, 7, (uint)value & 0x1F);
            BitField.SetField(ref _inst, 7, 25, ((uint)value >> 5) & 0x7F);
        }
    }

    /// <summary>
    /// B-Type Immediates (13-bit, scrambled, bit 0 is always 0)
    /// Used for: beq, bne, blt
    /// </summary>
    public int BranchOffset
    {
        readonly get
        {
            // Extract the 4 scrambled parts
            uint b1_4 = BitField.GetField(_inst, 4, 8);     // Inst[11:8]  -> Imm[4:1]
            uint b5_10 = BitField.GetField(_inst, 6, 25);   // Inst[30:25] -> Imm[10:5]
            uint b11 = BitField.GetField(_inst, 1, 7);      // Inst[7]     -> Imm[11]
            uint b12 = BitField.GetField(_inst, 1, 31);     // Inst[31]    -> Imm[12] (Sign)

            // Shift parts into their logical positions in a 13-bit value
            uint raw = (b12 << 12) | (b11 << 11) | (b5_10 << 5) | (b1_4 << 1);

            // Sign extend from bit 12 to 32 bits
            // Logic: Shift left to move bit 12 to bit 31, then arithmetic shift right 
            // to propagate the sign bit across the top 19 bits.
            return (int)(raw << 19) >> 19;
        }
        set
        {
            uint val = (uint)value;
            BitField.SetField(ref _inst, 4, 8, (val >> 1) & 0xF);   // bits 1-4
            BitField.SetField(ref _inst, 6, 25, (val >> 5) & 0x3F); // bits 5-10
            BitField.SetField(ref _inst, 1, 7, (val >> 11) & 0x1);  // bit 11
            BitField.SetField(ref _inst, 1, 31, (val >> 12) & 0x1); // bit 12
        }
    }

    /// <summary>
    /// U-Type Immediates (20-bit, bits 31:12)
    /// Used for: lui, auipc
    /// </summary>
    public int UpperImmediate
    {
        readonly get => (int)BitField.GetField(_inst, 20, 12);
        set => BitField.SetField(ref _inst, 20, 12, (uint)value & 0xF_FFFF);
    }

    /// <summary>
    /// J-Type Immediates (21-bit, scrambled, bit 0 is always 0)
    /// Used for: jal
    /// </summary>
    public int JumpOffset
    {
        readonly get
        {
            // Extract the 4 scrambled parts
            uint j1_10 = BitField.GetField(_inst, 10, 21);  // Inst[30:21] -> Imm[10:1]
            uint j11 = BitField.GetField(_inst, 1, 20);      // Inst[20]    -> Imm[11]
            uint j12_19 = BitField.GetField(_inst, 8, 12);   // Inst[19:12] -> Imm[19:12]
            uint j20 = BitField.GetField(_inst, 1, 31);      // Inst[31]    -> Imm[20] (Sign)

            // Assemble into a 21-bit raw value
            uint raw = (j20 << 20) | (j12_19 << 12) | (j11 << 11) | (j1_10 << 1);

            // Sign extend from bit 20 to 32 bits
            return (int)(raw << 11) >> 11;
        }
        set
        {
            uint val = (uint)value;
            // Map logical bits to instruction positions
            BitField.SetField(ref _inst, 10, 21, (val >> 1) & 0x3FF);  // bits 1-10
            BitField.SetField(ref _inst, 1, 20, (val >> 11) & 0x1);    // bit 11
            BitField.SetField(ref _inst, 8, 12, (val >> 12) & 0xFF);   // bits 12-19
            BitField.SetField(ref _inst, 1, 31, (val >> 20) & 0x1);    // bit 20
        }
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="RiscVInstruction"/>.
    /// </summary>
    public static explicit operator RiscVInstruction(uint value) => Unsafe.As<uint, RiscVInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="RiscVInstruction"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(RiscVInstruction value) => Unsafe.As<RiscVInstruction, uint>(ref value);
}
