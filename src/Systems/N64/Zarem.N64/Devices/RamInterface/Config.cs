// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// A struct for parsing the <see cref="RiRegister.Config"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/RDRAM_Interface#0x0470_0004_-_RI_CONFIG
/// </remarks>
public struct Config
{
    private const int CurrentControlOffset = 0;
    private const int CurrentControlSize = 6;
    private const int AutoCCBit = 6;

    private uint _value;

    /// <summary>
    /// Gets or sets the value to load into the current control register when <see cref="AutoCC"/> is disabled.
    /// </summary>
    public byte CurrentControl
    {
        readonly get => (byte)BitField.GetField(_value, CurrentControlSize, CurrentControlOffset);
        set => BitField.SetField(ref _value, CurrentControlSize, CurrentControlOffset, (byte)(value - 1));
    }

    /// <summary>
    /// Gets or sets whether or not automatic current calibration is enabled.
    /// </summary>
    public bool AutoCC
    {
        readonly get => BitField.GetBit(_value, AutoCCBit);
        set => BitField.SetBit(ref _value, AutoCCBit, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="Config"/>.
    /// </summary>
    public static explicit operator Config(uint value) => Unsafe.BitCast<uint, Config>(value);

    /// <summary>
    /// Casts a <see cref="Config"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(Config value) => Unsafe.BitCast<Config, uint>(value);
}
