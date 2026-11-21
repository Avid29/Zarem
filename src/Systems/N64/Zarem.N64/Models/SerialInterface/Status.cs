// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;
using Zarem.N64.Models.Enums.Registers;

namespace Zarem.N64.Models.SerialInterface;

/// <summary>
/// A struct for parsing the <see cref="SiRegister.Status"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/Serial_Interface
/// </remarks>
public struct Status
{
    private const int DmaBusyBit = 0;
    private const int IOBusyBit = 1;
    private const int ReadPendingBit = 2;
    private const int DmaErrorBit = 3;
    private const int ChannelStateOffset = 4;
    private const int ChannelStateSize = 4;
    private const int DmaStateOffset = 4;
    private const int DmaStateSize = 4;
    private const int InterruptBit = 12;

    private uint _value;

    /// <summary>
    /// Gets or sets whether or not a DMA read/write, or IO read/write, is in progrss.
    /// </summary>
    public bool DmaBusy
    {
        readonly get => BitField.GetBit(_value, DmaBusyBit);
        set => BitField.SetBit(ref _value, DmaBusyBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not a direct memory write to PIF RAM is in progress.
    /// </summary>
    public bool IOBusy
    {
        readonly get => BitField.GetBit(_value, IOBusyBit);
        set => BitField.SetBit(ref _value, IOBusyBit, value);
    }

    /// <summary>
    /// Unknown.
    /// </summary>
    public bool ReadPending
    {
        readonly get => BitField.GetBit(_value, ReadPendingBit);
        set => BitField.SetBit(ref _value, ReadPendingBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not a DMA error occured.
    /// </summary>
    /// <remarks>
    /// Examples: Overlapping DMA requests or writing to misaligned address.
    /// Only cleared by power reset.
    /// </remarks>
    public bool DmaError
    {
        readonly get => BitField.GetBit(_value, DmaErrorBit);
        set => BitField.SetBit(ref _value, DmaErrorBit, value);
    }

    /// <summary>
    /// Gets or sets the PIF channel state.
    /// </summary>
    public byte ChannelState
    {
        readonly get => (byte)BitField.GetField(_value, ChannelStateSize, ChannelStateOffset);
        set => BitField.SetField(ref _value, ChannelStateSize, ChannelStateOffset, value);
    }

    /// <summary>
    /// Gets or sets the internal DMA state.
    /// </summary>
    public byte DmaState
    {
        readonly get => (byte)BitField.GetField(_value, DmaStateSize, DmaStateOffset);
        set => BitField.SetField(ref _value, DmaStateSize, DmaStateOffset, value);
    }

    /// <summary>
    /// Gets or sets whether or not an interrupt occuored.
    /// </summary>
    public bool Interrupt
    {
        readonly get => BitField.GetBit(_value, InterruptBit);
        set => BitField.SetBit(ref _value, InterruptBit, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="Status"/>.
    /// </summary>
    public static explicit operator Status(uint value) => Unsafe.BitCast<uint, Status>(value);

    /// <summary>
    /// Casts a <see cref="Status"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(Status value) => Unsafe.BitCast<Status, uint>(value);
}
