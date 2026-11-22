// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;

namespace Zarem.N64.Devices.MipsInterface;

/// <summary>
/// A struct for parsing the <see cref="MiRegister.Mode"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/MIPS_Interface#0x0430_0000_-_MI_MODE
/// </remarks>
public struct Mode
{
    private const int RepeatCountOffset = 0;
    private const int RepeatCountSize = 7;
    private const int RepeatBit = 7;
    private const int EBusBit = 8;
    private const int UppperBit = 9;

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
    /// Gets or sets whether or not repeat mode is enable.
    /// </summary>
    /// <remarks>
    /// Automatically clears after a single Rambus write.
    /// </remarks>
    public bool Repeat
    {
        readonly get => BitField.GetBit(_value, RepeatBit);
        set => BitField.SetBit(ref _value, RepeatBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not EBus mode is enabled.
    /// </summary>
    public bool EBus
    {
        readonly get => BitField.GetBit(_value, EBusBit);
        set => BitField.SetBit(ref _value, EBusBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not upper mode is enabled.
    /// </summary>
    public bool Upper
    {
        readonly get => BitField.GetBit(_value, UppperBit);
        set => BitField.SetBit(ref _value, UppperBit, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="Mode"/>.
    /// </summary>
    public static explicit operator Mode(uint value) => Unsafe.BitCast<uint, Mode>(value);

    /// <summary>
    /// Casts a <see cref="Mode"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(Mode value) => Unsafe.BitCast<Mode, uint>(value);
}
