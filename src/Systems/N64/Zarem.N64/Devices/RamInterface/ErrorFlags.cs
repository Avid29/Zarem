// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// A flagged enum for parsing the <see cref="RiRegister.Error"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/RDRAM_Interface#0x0470_0018_-_RI_ERROR
/// </remarks>
[Flags]
public enum ErrorFlags
{
#pragma warning disable CS1591
    MissingAck = 0x1,
    UnexpectedNAck = 0x2,
    OverRangeError = 0x4,
#pragma warning restore CS1591
}
