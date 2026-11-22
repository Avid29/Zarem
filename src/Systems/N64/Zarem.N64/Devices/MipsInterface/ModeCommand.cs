// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;

namespace Zarem.N64.Devices.MipsInterface;

/// <summary>
/// A struct for parsing a command written to the <see cref="MiRegister.Mode"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/MIPS_Interface#0x0430_0000_-_MI_MODE
/// </remarks>
public struct ModeCommand
{
    private const int RepeatCountOffset = 0;
    private const int RepeatCountSize = 7;
    private const int ClearRepeatBit = 7;
    private const int SetRepeatBit = 8;
    private const int ClearEBusBit = 9;
    private const int SetEBusBit = 10;
    private const int ClearDPBit = 11;
    private const int ClearUpperBit = 12;
    private const int SetUpperBit = 13;

    private uint _value;

    /// <summary>
    /// Gets or sets the number of bytes to write in repeat mode.
    /// </summary>
    /// <remarks>
    /// Encoded as repeat-count minus one (-1).
    /// </remarks>
    public byte RepeatCount
    {
        readonly get => (byte)(BitField.GetField(_value, RepeatCountSize, RepeatCountOffset) + 1);
        set => BitField.SetField(ref _value, RepeatCountSize, RepeatCountOffset, (byte)(value - 1));
    }

    /// <summary>
    /// Gets or sets whether or not to clear repeat mode.
    /// </summary>
    public bool ClearRepeat
    {
        readonly get => BitField.GetBit(_value, ClearRepeatBit);
        set => BitField.SetBit(ref _value, ClearRepeatBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not to set repeat mode.
    /// </summary>
    /// <remarks>
    /// Automatically clears after a single Rambus write.
    /// </remarks>
    public bool SetRepeat
    {
        readonly get => BitField.GetBit(_value, SetRepeatBit);
        set => BitField.SetBit(ref _value, SetRepeatBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not to clear EBus mode.
    /// </summary>
    public bool ClearEBus
    {
        readonly get => BitField.GetBit(_value, ClearEBusBit);
        set => BitField.SetBit(ref _value, ClearEBusBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not to set EBus mode.
    /// </summary>
    public bool SetEBus
    {
        readonly get => BitField.GetBit(_value, SetEBusBit);
        set => BitField.SetBit(ref _value, SetEBusBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not to clear DP mode.
    /// </summary>
    public bool ClearDP
    {
        readonly get => BitField.GetBit(_value, ClearDPBit);
        set => BitField.SetBit(ref _value, ClearDPBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not to clear upper mode.
    /// </summary>
    public bool ClearUpper
    {
        readonly get => BitField.GetBit(_value, ClearUpperBit);
        set => BitField.SetBit(ref _value, ClearUpperBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not to set upper mode.
    /// </summary>
    public bool SetUpper
    {
        readonly get => BitField.GetBit(_value, SetUpperBit);
        set => BitField.SetBit(ref _value, SetUpperBit, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="ModeCommand"/>.
    /// </summary>
    public static explicit operator ModeCommand(uint value) => Unsafe.BitCast<uint, ModeCommand>(value);

    /// <summary>
    /// Casts a <see cref="ModeCommand"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(ModeCommand value) => Unsafe.BitCast<ModeCommand, uint>(value);
}
