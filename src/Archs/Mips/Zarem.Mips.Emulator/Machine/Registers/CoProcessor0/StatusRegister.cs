// Avishai Dernis 2025

using System.Runtime.CompilerServices;
using Zarem.Helpers;
using Zarem.Mips.Emulator.Machine.Enums;

namespace Zarem.Mips.Emulator.Machine.Registers.CoProcessor0;

/// <summary>
/// CoProcessor0 Status register.
/// </summary>
/// <remarks>
/// Controls processor mode, interrupt enabling, and exception state.
/// </remarks>
public struct StatusRegister
{
    private const int INTERUPT_ENABLED_BIT = 0;
    private const int EXCEPTION_LEVEL_BIT = 1;
    private const int ERROR_LEVEL_BIT = 2;

    private const int KSU_OFFSET = 3;
    private const int KSU_SIZE = 2;

    private const int INTERUPT_MASK_SIZE = 8;
    private const int INTERUPT_MASK_OFFSET = 8;

    private const int BOOTSTRAPPING_BIT = 22;
    private const int FLOATINGPOINT64_MODE_BIT = 26;

    private uint _status;

    /// <summary>
    /// Gets or sets if interupts are enabled.
    /// </summary>
    /// <remarks>
    /// Interrupts are only taken when <see cref="InteruptEnabled"/> is <see langword="true"/> and <see cref="ExceptionLevel"/> is <see langword="false"/>."/>
    /// </remarks>
    public bool InteruptEnabled
    {
        readonly get => BitField.GetBit(_status, INTERUPT_ENABLED_BIT);
        set => BitField.SetBit(ref _status, INTERUPT_ENABLED_BIT, value);
    }

    /// <summary>
    /// Gets or sets the exception level.
    /// </summary>
    /// <remarks>
    /// Set on exception entry. Cleared by eret.
    /// </remarks>
    public bool ExceptionLevel
    {
        readonly get => BitField.GetBit(_status, EXCEPTION_LEVEL_BIT);
        set => BitField.SetBit(ref _status, EXCEPTION_LEVEL_BIT, value);
    }

    /// <summary>
    /// Gets or sets the error level.
    /// </summary>
    /// <remarks>
    /// Used for reset and NMI handling.
    /// </remarks>
    public bool ErrorLevel
    {
        readonly get => BitField.GetBit(_status, ERROR_LEVEL_BIT);
        set => BitField.SetBit(ref _status, ERROR_LEVEL_BIT, value);
    }

    /// <summary>
    /// Gets or sets the processor privilege mode.
    /// </summary>
    /// <remarks>
    /// The effective status is <see cref="PrivilegeMode.Kernel"/> regardless of the <see cref="PrivilegeMode"/> when <see cref="ExceptionLevel"/> or <see cref="ErrorLevel"/> is <see langword="true"/>.
    /// </remarks>
    public PrivilegeMode PrivilegeMode
    {
        readonly get => (PrivilegeMode)BitField.GetField(_status, KSU_SIZE, KSU_OFFSET);
        set => BitField.SetField(ref _status, KSU_SIZE, KSU_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets or sets the interupt mask.
    /// </summary>
    /// <remarks>
    /// Each bit masks a corresponding interrupt line.
    /// </remarks>
    public byte InteruptMask
    {
        readonly get => (byte)BitField.GetField(_status, INTERUPT_MASK_SIZE, INTERUPT_MASK_OFFSET);
        set => BitField.SetField(ref _status, INTERUPT_MASK_SIZE, INTERUPT_MASK_OFFSET, value);
    }

    /// <summary>
    /// Gets or sets if the system is in bootstrapping mode.
    /// </summary>
    public bool BootStrapping
    {
        readonly get => BitField.GetBit(_status, BOOTSTRAPPING_BIT);
        set => BitField.SetBit(ref _status, BOOTSTRAPPING_BIT, value);
    }

    /// <summary>
    /// Gets or sets if the floating-point registers are in 32bit pair or 64bit mode.
    /// </summary>
    public bool FloatingPoint64BitMode
    {
        readonly get => BitField.GetBit(_status, FLOATINGPOINT64_MODE_BIT);
        set => BitField.SetBit(ref _status, FLOATINGPOINT64_MODE_BIT, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="StatusRegister"/>.
    /// </summary>
    public static explicit operator StatusRegister(uint value) => Unsafe.As<uint, StatusRegister>(ref value);

    /// <summary>
    /// Casts a <see cref="StatusRegister"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(StatusRegister value) => Unsafe.As<StatusRegister, uint>(ref value);
}
