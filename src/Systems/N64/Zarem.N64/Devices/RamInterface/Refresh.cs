// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Helpers;

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// A struct for parsing the <see cref="RiRegister.Refresh"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/RDRAM_Interface#0x0470_0010_-_RI_REFRESH
/// </remarks>
public struct Refresh
{
    private const int CleanRefreshDelayOffset = 0;
    private const int CleanRefreshDelaySize = 8;
    private const int DirtyRefreshDelayOffset = 8;
    private const int DirtyRefreshDelaySize = 8;
    private const int BankBit = 16;
    private const int EnabledBit = 17;
    private const int OptimizeBit = 18;
    private const int MultiBankOffset = 19;
    private const int MultiBankSize = 4;

    private uint _value;

    /// <summary>
    /// Gets or sets a value the cycles to delay after refresh when the bank was previously clean.
    /// </summary>
    /// <remarks>
    /// Usually set to 52.
    /// </remarks>
    public byte CleanRefreshDelay
    {
        readonly get => (byte)BitField.GetField(_value, CleanRefreshDelaySize, CleanRefreshDelayOffset);
        set => BitField.SetField(ref _value, CleanRefreshDelaySize, CleanRefreshDelayOffset, value);
    }

    /// <summary>
    /// Gets or sets a value the cycles to delay after refresh when the bank was previously dirty.
    /// </summary>
    /// <remarks>
    /// Usually set to 52.
    /// </remarks>
    public byte DirtyRefreshDelay
    {
        readonly get => (byte)BitField.GetField(_value, DirtyRefreshDelaySize, DirtyRefreshDelayOffset);
        set => BitField.SetField(ref _value, DirtyRefreshDelaySize, DirtyRefreshDelayOffset, value);
    }

    /// <summary>
    /// ???
    /// </summary>
    /// <remarks>
    /// "Oscillates between 0 and 1 during operation."
    /// </remarks>
    public bool Bank
    {
        readonly get => BitField.GetBit(_value, BankBit);
        set => BitField.SetBit(ref _value, BankBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not automatic refresh is enabled.
    /// </summary>
    public bool Enabled
    {
        readonly get => BitField.GetBit(_value, EnabledBit);
        set => BitField.SetBit(ref _value, EnabledBit, value);
    }

    /// <summary>
    /// Gets or sets whether or not optimization is enabled.
    /// </summary>
    public bool Optimize
    {
        readonly get => BitField.GetBit(_value, OptimizeBit);
        set => BitField.SetBit(ref _value, OptimizeBit, value);
    }

    /// <summary>
    /// Bitfield indicating multibank RDRAM modules.
    /// </summary>
    public byte MultiBank
    {
        readonly get => (byte)BitField.GetField(_value, MultiBankSize, MultiBankOffset);
        set => BitField.SetField(ref _value, MultiBankSize, MultiBankOffset, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="Refresh"/>.
    /// </summary>
    public static explicit operator Refresh(uint value) => Unsafe.BitCast<uint, Refresh>(value);

    /// <summary>
    /// Casts a <see cref="Refresh"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(Refresh value) => Unsafe.BitCast<Refresh, uint>(value);
}
