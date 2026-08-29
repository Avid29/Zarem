// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.AArch64.Models.Instructions.Enums.Registers;
using Zarem.Helpers;

namespace Zarem.AArch64.Models.Instructions;

/// <summary>
/// A struct representing an AArch64 instruction.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct AArch64Instruction
{
    // Universal Core Class Opcode Bits
    private const int OPCODE_BIT_SIZE = 4;
    private const int OPCODE_BIT_OFFSET = 25;

    // Standard Register/Immediate Bit Sizes
    private const int REG_BIT_SIZE = 5;
    private const int IMM5_BIT_SIZE = 5;
    private const int IMM6_BIT_SIZE = 6;
    private const int IMM12_BIT_SIZE = 12;
    private const int IMM16_BIT_SIZE = 16;
    private const int IMM19_BIT_SIZE = 19;
    private const int IMM26_BIT_SIZE = 26;

    // Standard Field Offsets
    private const int RD_BIT_OFFSET = 0;   // Often Rd, Rt, or Rt2
    private const int RN_BIT_OFFSET = 5;   // Often Rn or base register
    private const int IMM_BIT_OFFSET = 10; // Shift, scale, or middle immediates
    private const int RM_BIT_OFFSET = 16;  // Second source register
    private const int HW_BIT_OFFSET = 21;  // High-order sub-opcodes / Shift types

    private const int SF_BIT = 32;

    [FieldOffset(0)]
    private uint _inst;

    /// <summary>
    /// Gets the destination register.
    /// </summary>
    public AArch64GpRegister RD
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (AArch64GpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RD_BIT_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RD_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the first source register.
    /// </summary>
    public AArch64GpRegister RN
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (AArch64GpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RN_BIT_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RN_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the second source register.
    /// </summary>
    public AArch64GpRegister RM
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (AArch64GpRegister)BitField.GetField(_inst, REG_BIT_SIZE, RM_BIT_OFFSET);
        set => BitField.SetField(ref _inst, REG_BIT_SIZE, RM_BIT_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets the 12-bit immediate used in Add/Sub immediate instructions.
    /// </summary>
    public ushort Imm12
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (ushort)BitField.GetField(_inst, IMM12_BIT_SIZE, IMM_BIT_OFFSET);
        set => BitField.SetField(ref _inst, IMM12_BIT_SIZE, IMM_BIT_OFFSET, value);
    }

    /// <summary>
    /// Gets the 16-bit immediate used in Move Wide (MOVZ, MOVK, MOVN) instructions.
    /// </summary>
    public ushort Imm16
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (ushort)BitField.GetField(_inst, IMM16_BIT_SIZE, RN_BIT_OFFSET);
        set => BitField.SetField(ref _inst, IMM16_BIT_SIZE, RN_BIT_OFFSET, value);
    }

    /// <summary>
    /// Gets the 19-bit signed branch offset used in conditional branches (B.cond).
    /// </summary>
    public int BranchOffset19
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (int)BitField.GetField(_inst, IMM19_BIT_SIZE, RN_BIT_OFFSET, true) << 2;
        set => BitField.SetField(ref _inst, IMM19_BIT_SIZE, RN_BIT_OFFSET, (uint)((value >> 2) & 0x7FFFF));
    }

    /// <summary>
    /// Gets the 26-bit signed branch offset used in unconditional branches (B, BL).
    /// </summary>
    public int BranchOffset26
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (int)BitField.GetField(_inst, IMM26_BIT_SIZE, 0, true) << 2;
        set => BitField.SetField(ref _inst, IMM26_BIT_SIZE, 0, (uint)((value >> 2) & 0x3FFFFFF));
    }

    /// <summary>
    /// Gets the SF bit which determines if this is a 64-bit operation (1) or a 32-bit operation (0).
    /// </summary>
    public bool Is64Bit
    {
        readonly get => BitField.GetBit(_inst, SF_BIT);
        set => BitField.SetBit(ref _inst, SF_BIT, value);
    }
}
