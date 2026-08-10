// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;
using Zarem.Models.Interface;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Models.Instructions;

/// <summary>
/// A struct representing a compressed RISC-V instruction.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 2)]
public struct RiscVCompressedInstruction : IInstruction
{
    private const byte COMPCODE_BIT_SIZE = 2;
    private const byte REG_BIT_SIZE = 5;
    private const byte COMPRESSED_REG_BIT_SIZE = 3;
    private const byte FUNCT3_BIT_SIZE = 3;
    private const byte FUNCT4_BIT_SIZE = 4;

    private const byte COMPCODE_OFFSET = 0;
    private const byte RS2_OFFSET = 2;
    private const byte RD_COMPRESSED_OFFSET = 2;
    private const byte RDRS1_OFFSET = 7;
    private const byte RS1_COMPRESSED_OFFSET = 7;
    private const byte FUNCT4_OFFSET = 12;
    private const byte FUNCT3_OFFSET = 13;

    private const ushort COMPRESSION_MASK = 0b0111;
    private const ushort COMPRESSION_APPEND = 0b1000;

    [FieldOffset(0)]
    private ushort _inst;

    /// <summary>
    /// Creates a CR-Type instruction.
    /// </summary>
    public static RiscVCompressedInstruction CreateCR(RiscVCompressionCode comp, CFunct4Code cf4, RiscVGpRegister rdrs1, RiscVGpRegister rs2)
    {
        return new()
        {
            CompressionCode = comp,
            Funct4 = cf4,
            RDRS1 = rdrs1,
            RS2 = rs2,
        };
    }

    /// <summary>
    /// Creates a CI-Type instruction.
    /// </summary>
    public static RiscVCompressedInstruction CreateCI(RiscVCompressionCode comp, CFunct3Code cf3, RiscVGpRegister rdrs1, sbyte imm)
    {
        return new()
        {
            CompressionCode = comp,
            Funct3 = cf3,
            RDRS1 = rdrs1,
            Immediate = imm,
        };
    }

    /// <summary>
    /// Gets or sets the instruction's compression code.
    /// </summary>
    public RiscVCompressionCode CompressionCode
    {
        readonly get => (RiscVCompressionCode)BitField.GetField(_inst, COMPCODE_BIT_SIZE, COMPCODE_OFFSET);
        set => BitField.SetField(ref _inst, COMPCODE_BIT_SIZE, COMPCODE_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets or sets the instruction's destination/source1 register.
    /// </summary>
    public RiscVGpRegister RDRS1
    {
        readonly get => (RiscVGpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RDRS1_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RDRS1_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets or sets the instruction's second source register.
    /// </summary>
    public RiscVGpRegister RS2
    {
        readonly get => (RiscVGpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS2_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS2_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets or sets the instruction's compressed RD register.
    /// </summary>
    public RiscVGpRegister RD_Compressed
    {
        readonly get => (RiscVGpRegister)(BitField.GetField(_inst, COMPRESSED_REG_BIT_SIZE, RD_COMPRESSED_OFFSET) | COMPRESSION_APPEND);
        set => BitField.SetField(ref _inst, COMPRESSED_REG_BIT_SIZE, RD_COMPRESSED_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets or sets the instruction's compressed RS1 register.
    /// </summary>
    public RiscVGpRegister RS1_Compressed
    {
        readonly get => (RiscVGpRegister)(BitField.GetField(_inst, COMPRESSED_REG_BIT_SIZE, RS1_COMPRESSED_OFFSET) | COMPRESSION_APPEND);
        set => BitField.SetField(ref _inst, COMPRESSED_REG_BIT_SIZE, RS1_COMPRESSED_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets or sets the value of the funct3 field.
    /// </summary>
    public CFunct3Code Funct3
    {
        readonly get => (CFunct3Code)BitField.GetField(_inst, FUNCT3_BIT_SIZE, FUNCT3_OFFSET);
        set => BitField.SetField(ref _inst, FUNCT3_BIT_SIZE, FUNCT3_OFFSET, (ushort)value);
    }

    /// <summary>
    /// Gets or sets the value of the funct4 field.
    /// </summary>
    public CFunct4Code Funct4
    {
        readonly get => (CFunct4Code)BitField.GetField(_inst, FUNCT4_BIT_SIZE, FUNCT4_OFFSET);
        set => BitField.SetField(ref _inst, FUNCT4_BIT_SIZE, FUNCT4_OFFSET, (ushort)value);
    }

    /// <summary>
    /// CI-Format Immediate (Imm[5] at bit 12, Imm[4:0] at bits [6:2], 6-bit signed).
    /// </summary>
    public sbyte Immediate
    {
        readonly get
        {
            byte imm0_4 = (byte)BitField.GetField(_inst, 5, 2);
            byte imm5 = (byte)BitField.GetField(_inst, 1, 12);
            byte raw = (byte)((imm5 << 5) | imm0_4);
            return (sbyte)((raw << 2) >> 2);
        }
        set
        {
            byte val = (byte)value;
            BitField.SetField(ref _inst, 5, 2, (ushort)(val & 0x1F));
            BitField.SetField(ref _inst, 1, 12, (ushort)((val >> 5) & 0x1));
        }
    }

    /// <summary>
    /// CSS-Format Stack-Store Offset for c.swsp (Imm[5:2] at [12:9], Imm[7:6] at [8:7], scaled by 4).
    /// </summary>
    public byte StackStoreOffset
    {
        readonly get
        {
            byte b5_2 = (byte)BitField.GetField(_inst, 4, 9);
            byte b7_6 = (byte)BitField.GetField(_inst, 2, 7);
            return (byte)((b7_6 << 6) | (b5_2 << 2));
        }
        set
        {
            BitField.SetField(ref _inst, 4, 9, (ushort)((value >> 2) & 0xF));
            BitField.SetField(ref _inst, 2, 7, (ushort)((value >> 6) & 0x3));
        }
    }

    /// <summary>
    /// CIW-Format Wide Stack Pointer Offset for c.addi4spn (Imm[5:4|9:6|2|3] across [12:5], scaled by 4).
    /// </summary>
    public ushort StackOffset
    {
        readonly get
        {
            byte b5_4 = (byte)BitField.GetField(_inst, 2, 11);
            byte b9_6 = (byte)BitField.GetField(_inst, 4, 7);
            byte b2 = (byte)BitField.GetField(_inst, 1, 6);
            byte b3 = (byte)BitField.GetField(_inst, 1, 5);
            return (ushort)((b9_6 << 6) | (b5_4 << 4) | (b3 << 3) | (b2 << 2));
        }
        set
        {
            BitField.SetField(ref _inst, 2, 11, (ushort)((value >> 4) & 0x3));
            BitField.SetField(ref _inst, 4, 7, (ushort)((value >> 6) & 0xF));
            BitField.SetField(ref _inst, 1, 6, (ushort)((value >> 2) & 0x1));
            BitField.SetField(ref _inst, 1, 5, (ushort)((value >> 3) & 0x1));
        }
    }

    /// <summary>
    /// CL/CS-Format Word Load/Store Offset for c.lw/c.sw (Imm[5:3|2|6] across [12:10|6|5], scaled by 4).
    /// </summary>
    public byte LoadStoreOffset
    {
        readonly get
        {
            byte b5_3 = (byte)BitField.GetField(_inst, 3, 10);
            byte b2 = (byte)BitField.GetField(_inst, 1, 6);
            byte b6 = (byte)BitField.GetField(_inst, 1, 5);
            return (byte)((b6 << 6) | (b5_3 << 3) | (b2 << 2));
        }
        set
        {
            BitField.SetField(ref _inst, 3, 10, (ushort)((value >> 3) & 0x7));
            BitField.SetField(ref _inst, 1, 6, (ushort)((value >> 2) & 0x1));
            BitField.SetField(ref _inst, 1, 5, (ushort)((value >> 6) & 0x1));
        }
    }

    /// <summary>
    /// CB-Format Branch Offset for c.beqz/c.bnez (Imm[8|7:6|5|4:3|2:1], scaled by 2, bit 0 implicitly 0).
    /// Range: -256 to +254 bytes.
    /// </summary>
    public short BranchOffset
    {
        readonly get
        {
            byte b8 = (byte)BitField.GetField(_inst, 1, 12);
            byte b4_3 = (byte)BitField.GetField(_inst, 2, 10);
            byte b7_6 = (byte)BitField.GetField(_inst, 2, 5);
            byte b2_1 = (byte)BitField.GetField(_inst, 2, 3);
            byte b5 = (byte)BitField.GetField(_inst, 1, 2);

            ushort raw = (ushort)((b8 << 8) | (b7_6 << 6) | (b5 << 5) | (b4_3 << 3) | (b2_1 << 1));
            return (short)((int)(raw << 23) >> 23);
        }
        set
        {
            ushort val = (ushort)value;
            BitField.SetField(ref _inst, 1, 12, (ushort)((val >> 8) & 0x1));
            BitField.SetField(ref _inst, 2, 10, (ushort)((val >> 3) & 0x3));
            BitField.SetField(ref _inst, 2, 5, (ushort)((val >> 6) & 0x3));
            BitField.SetField(ref _inst, 2, 3, (ushort)((val >> 1) & 0x3));
            BitField.SetField(ref _inst, 1, 2, (ushort)((val >> 5) & 0x1));
        }
    }

    /// <summary>
    /// CJ-Format Jump Offset for c.j/c.jal (Imm[11|10|9:8|7|6|5|4|3:1], scaled by 2, bit 0 implicitly 0).
    /// Range: -2048 to +2046 bytes.
    /// </summary>
    public short JumpOffset
    {
        readonly get
        {
            byte b11 = (byte)BitField.GetField(_inst, 1, 12);
            byte b4 = (byte)BitField.GetField(_inst, 1, 11);
            byte b9_8 = (byte)BitField.GetField(_inst, 2, 9);
            byte b10 = (byte)BitField.GetField(_inst, 1, 8);
            byte b6 = (byte)BitField.GetField(_inst, 1, 7);
            byte b7 = (byte)BitField.GetField(_inst, 1, 6);
            byte b3_1 = (byte)BitField.GetField(_inst, 3, 3);
            byte b5 = (byte)BitField.GetField(_inst, 1, 2);

            ushort raw = (ushort)((b11 << 11) | (b10 << 10) | (b9_8 << 8) | (b7 << 7) |
                                  (b6 << 6) | (b5 << 5) | (b4 << 4) | (b3_1 << 1));
            return (short)((int)(raw << 20) >> 20);
        }
        set
        {
            ushort val = (ushort)value;
            BitField.SetField(ref _inst, 1, 12, (ushort)((val >> 11) & 0x1));
            BitField.SetField(ref _inst, 1, 11, (ushort)((val >> 4) & 0x1));
            BitField.SetField(ref _inst, 2, 9, (ushort)((val >> 8) & 0x3));
            BitField.SetField(ref _inst, 1, 8, (ushort)((val >> 10) & 0x1));
            BitField.SetField(ref _inst, 1, 7, (ushort)((val >> 6) & 0x1));
            BitField.SetField(ref _inst, 1, 6, (ushort)((val >> 7) & 0x1));
            BitField.SetField(ref _inst, 3, 3, (ushort)((val >> 1) & 0x7));
            BitField.SetField(ref _inst, 1, 2, (ushort)((val >> 5) & 0x1));
        }
    }

    /// <inheritdoc/>
    public readonly int Length => ((RiscVInstruction)this).Length;

    /// <summary>
    /// Casts a <see cref="ushort"/> to a <see cref="RiscVCompressedInstruction"/>.
    /// </summary>
    public static explicit operator RiscVCompressedInstruction(ushort value) => Unsafe.As<ushort, RiscVCompressedInstruction>(ref value);

    /// <summary>
    /// Casts a <see cref="RiscVCompressedInstruction"/> to a <see cref="ushort"/>.
    /// </summary>
    public static explicit operator ushort(RiscVCompressedInstruction value) => Unsafe.As<RiscVCompressedInstruction, ushort>(ref value);

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="RiscVCompressedInstruction"/>.
    /// </summary>
    public static explicit operator RiscVCompressedInstruction(uint value) => (RiscVCompressedInstruction)(ushort)value;

    /// <summary>
    /// Casts a <see cref="RiscVCompressedInstruction"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(RiscVCompressedInstruction value) => (ushort)value;

    /// <summary>
    /// Casts a <see cref="RiscVCompressedInstruction"/> to a <see cref="RiscVInstruction"/>.
    /// </summary>
    public static implicit operator RiscVInstruction(RiscVCompressedInstruction value) => (RiscVInstruction)(uint)value;

    /// <summary>
    /// Casts a <see cref="RiscVCompressedInstruction"/> to a <see cref="RiscVInstruction"/>.
    /// </summary>
    public static explicit operator RiscVCompressedInstruction(RiscVInstruction value) => (RiscVCompressedInstruction)(uint)value;
}
