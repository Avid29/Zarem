// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Enums;

namespace Zarem.Mips.Emulator.Machine.Registers.FloatProcessor;

/// <summary>
/// A struct representing the master MIPS Floating-Point Control and Status Register (FCSR / FCR31).
/// </summary>
public struct FcsrRegister
{
    private const int RM_OFFSET = 0;
    private const int RM_SIZE = 2;

    private const int FLAGS_OFFSET = 2;
    private const int FLAGS_SIZE = 5;

    private const int ENABLES_OFFSET = 7;
    private const int ENABLES_SIZE = 5;

    private const int CAUSE_OFFSET = 12;
    private const int CAUSE_SIZE = 6;

    private const int FCC0_BIT = 23;
    private const int FS_BIT = 24;

    private const int FCC7_1_OFFSET = 25;
    private const int FCC7_1_SIZE = 7;

    private uint _fcsr;

    /// <summary>
    /// Gets or sets the IEEE 754 Rounding Mode tracking selection.
    /// </summary>
    public MipsFpuRoundingMode RoundingMode
    {
        readonly get => (MipsFpuRoundingMode)BitField.GetField(_fcsr, RM_SIZE, RM_OFFSET);
        set => BitField.SetField(ref _fcsr, RM_SIZE, RM_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the cumulative exception flags byte mask (Inexact, Underflow, Overflow, DivByZero, Invalid).
    /// </summary>
    public MipsFpuException Flags
    {
        readonly get => (MipsFpuException)BitField.GetField(_fcsr, FLAGS_SIZE, FLAGS_OFFSET);
        set => BitField.SetField(ref _fcsr, FLAGS_SIZE, FLAGS_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the exception trap enables bit mask.
    /// </summary>
    public MipsFpuException Enables
    {
        readonly get => (MipsFpuException)BitField.GetField(_fcsr, ENABLES_SIZE, ENABLES_OFFSET);
        set => BitField.SetField(ref _fcsr, ENABLES_SIZE, ENABLES_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets the current execution trap cause bits (adds Unimplemented Operation at bit 17).
    /// </summary>
    public MipsFpuException Cause
    {
        readonly get => (MipsFpuException)BitField.GetField(_fcsr, CAUSE_SIZE, CAUSE_OFFSET);
        set => BitField.SetField(ref _fcsr, CAUSE_SIZE, CAUSE_OFFSET, (byte)value);
    }

    /// <summary>
    /// Gets or sets Flush-to-Zero / Denormals-are-Zero mode handling.
    /// </summary>
    public bool FlushToZero
    {
        readonly get => BitField.GetBit(_fcsr, FS_BIT);
        set => BitField.SetBit(ref _fcsr, FS_BIT, value);
    }

    /// <summary>
    /// Gets or sets the combined raw byte tracking all 8 Floating-Point Condition Codes (FCC7:0).
    /// Handles the physical split layout mapping internally.
    /// </summary>
    public byte ConditionCodes
    {
        readonly get
        {
            uint fcc0 = BitField.GetBit(_fcsr, FCC0_BIT) ? 1U : 0U;
            uint fcc7_1 = BitField.GetField(_fcsr, FCC7_1_SIZE, FCC7_1_OFFSET);
            return (byte)(fcc0 | (fcc7_1 << 1));
        }
        set
        {
            BitField.SetBit(ref _fcsr, FCC0_BIT, (value & 1U) != 0);
            BitField.SetField(ref _fcsr, FCC7_1_SIZE, FCC7_1_OFFSET, (uint)(value >> 1) & 0x7FU);
        }
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="FcsrRegister"/>.
    /// </summary>
    public static explicit operator FcsrRegister(uint value) => Unsafe.As<uint, FcsrRegister>(ref value);

    /// <summary>
    /// Casts a <see cref="FcsrRegister"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(FcsrRegister value) => Unsafe.As<FcsrRegister, uint>(ref value);
}
