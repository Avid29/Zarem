// Avishai Dernis 2024

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Models.Instructions;

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
    private const int FUNCT3_BIT_SIZE = 3;
    private const int FUNCT5_BIT_SIZE = 5;

    private const int OPCODE_OFFSET = 0;
    private const int RD_OFFSET = 7;
    private const int FUNCT3_OFFSET = 12;
    private const int RS1_OFFSET = 15;
    private const int RS2_OFFSET = 20;
    private const int FUNCT5_OFFSET = 25;
    private const int FMT_OFFSET = 25;
    private const int RS3_OFFSET = 27;

    [FieldOffset(0)]
    private uint _inst;

    /// <summary>
    /// Creates a new floating-point instruction.
    /// </summary>
    public static RiscVFloatInstruction Create(RiscVOpCode opCode, RiscVFloatFormat format, FloatFunc5Code funct5, RiscVFloatRegister rd, RiscVFloatRegister rs1, RiscVFloatRegister rs2, FloatFunct3Code funct3 = FloatFunct3Code.RoundToNearest)
    {
        return new()
        {
            OpCode = opCode,
            Format = format,
            Funct5 = funct5,
            Funct3 = funct3,
            RD = rd,
            RS1 = rs1,
            RS2 = rs2,
        };
    }

    /// <summary>
    /// Creates a new floating-point instruction.
    /// </summary>
    public static RiscVFloatInstruction Create(RiscVOpCode opCode, RiscVFloatFormat format, RiscVFloatRegister rd, RiscVFloatRegister rs1, RiscVFloatRegister rs2, RiscVFloatRegister rs3, FloatFunct3Code funct3 = FloatFunct3Code.RoundToNearest)
    {
        return new()
        {
            OpCode = opCode,
            Format = format,
            Funct3 = funct3,
            RD = rd,
            RS1 = rs1,
            RS2 = rs2,
            RS3 = rs3,
        };
    }

    /// <summary>
    /// Gets or sets the instruction's operation code.
    /// </summary>
    public RiscVOpCode OpCode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (RiscVOpCode)BitField.GetField(_inst, OPCODE_BIT_SIZE, OPCODE_OFFSET);
        set => BitField.SetField(ref _inst, OPCODE_BIT_SIZE, OPCODE_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's format.
    /// </summary>
    public RiscVFloatFormat Format
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (RiscVFloatFormat)BitField.GetField(_inst, FMT_BIT_SIZE, FMT_OFFSET);
        set => BitField.SetField(ref _inst, FMT_BIT_SIZE, FMT_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's funct5 code.
    /// </summary>
    public FloatFunc5Code Funct5
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (FloatFunc5Code)BitField.GetField(_inst, FUNCT5_BIT_SIZE, FUNCT5_OFFSET);
        set => BitField.SetField(ref _inst, FUNCT5_BIT_SIZE, FUNCT5_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's funct3 code.
    /// </summary>
    public FloatFunct3Code Funct3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (FloatFunct3Code)BitField.GetField(_inst, FUNCT3_BIT_SIZE, FUNCT3_OFFSET);
        set => BitField.SetField(ref _inst, FUNCT3_BIT_SIZE, FUNCT3_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RD register.
    /// </summary>
    public RiscVFloatRegister RD
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (RiscVFloatRegister)BitField.GetField(_inst, REG_BIT_SIZE, RD_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RD_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RS1 register.
    /// </summary>
    public RiscVFloatRegister RS1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (RiscVFloatRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS1_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS1_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RS2 register.
    /// </summary>
    public RiscVFloatRegister RS2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (RiscVFloatRegister)BitField.GetField(_inst, REG_BIT_SIZE, RS2_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RS2_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the instruction's RS3 register.
    /// </summary>
    public RiscVFloatRegister RS3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
