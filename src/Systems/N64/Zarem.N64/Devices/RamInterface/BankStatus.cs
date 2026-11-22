// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// A struct for parsing the <see cref="RiRegister.BankStatus"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/RDRAM_Interface#0x0470_001c_-_RI_BANK_STATUS
/// </remarks>
public struct BankStatus
{
    private const int BankValidOffset = 0;
    private const int BankValidSize = 8;
    private const int BankDirtyOffset = 8;
    private const int BankDirtySize = 8;

    private uint _value;

    /// <summary>
    /// Gets or sets the valid bits for a bank.
    /// </summary>
    /// <remarks>
    /// One per bank. Set when a row is opened. Presumably only cleared by a refresh cycle
    /// </remarks>
    public BankFlags BankValid
    {
        readonly get => (BankFlags)BitField.GetField(_value, BankValidSize, BankValidOffset);
        set => BitField.SetField(ref _value, BankValidSize, BankValidOffset, (byte)value);
    }

    /// <summary>
    /// Gets or sets the dirty bit for a bank.
    /// </summary>
    /// <remarks>
    /// Set when the currently open row has been written. Cleared when a new row is opened but not yet written to.
    /// </remarks>
    public BankFlags BankDirty
    {
        readonly get => (BankFlags)BitField.GetField(_value, BankDirtySize, BankDirtyOffset);
        set => BitField.SetField(ref _value, BankDirtySize, BankDirtyOffset, (byte)value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="BankStatus"/>.
    /// </summary>
    public static explicit operator BankStatus(uint value) => Unsafe.BitCast<uint, BankStatus>(value);

    /// <summary>
    /// Casts a <see cref="BankStatus"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(BankStatus value) => Unsafe.BitCast<BankStatus, uint>(value);
}
