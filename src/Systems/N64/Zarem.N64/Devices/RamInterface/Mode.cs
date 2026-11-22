// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// A struct for parsing the <see cref="RiRegister.Mode"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/RDRAM_Interface#0x0470_0000_-_RI_MODE
/// </remarks>
public struct Mode
{
    private const int OpModeOffset = 0;
    private const int OpModeSize = 7;
    private const int StopRBit = 7;
    private const int StopTBit = 8;

    private uint _value;

    /// <summary>
    /// Gets or sets a value indicating how Serial Mode (SMode) packets are sent to RDRAM modules.
    /// </summary>
    /// <remarks>
    /// Usually set to <see cref="OpMode.Mode10"/>.
    /// </remarks>
    public OpMode OpMode
    {
        readonly get => (OpMode)BitField.GetField(_value, OpModeSize, OpModeOffset);
        set => BitField.SetField(ref _value, OpModeSize, OpModeOffset, (byte)value);
    }

    /// <summary>
    /// Gets or sets whether or not repeat mode is enabled.
    /// </summary>
    /// <remarks>
    /// Automatically clears after a single Rambus write.
    /// </remarks>
    public bool StopR
    {
        readonly get => BitField.GetBit(_value, StopRBit);
        set => BitField.SetBit(ref _value, StopRBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not EBus mode is enabled.
    /// </summary>
    public bool StopT
    {
        readonly get => BitField.GetBit(_value, StopTBit);
        set => BitField.SetBit(ref _value, StopTBit, value);
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
