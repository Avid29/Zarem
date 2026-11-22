// Avishai Dernis 2026

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// An enum for the <see cref="Mode.OpMode"/> field in the <see cref="RiRegister.Mode"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/RDRAM_Interface#0x0470_0000_-_RI_MODE
/// </remarks>
public enum OpMode : byte
{
    /// <summary>
    /// Sends continuous packages.
    /// </summary>
    Continuous = Mode00,

#pragma warning disable CS1591
    Mode00 = 0b00,
    Mode01 = 0b01,
    Mode10 = 0b10,
#pragma warning restore CS1591
}
