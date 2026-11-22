// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.ParallelInterface;

/// <summary>
/// An enum for parsing the <see cref="PiRegister.Status"/> register.
/// </summary>
[Flags]
public enum StatusFlags : uint
{
#pragma warning disable CS1591
    Busy = 0x1,
    IOBusy = 0x2,
    DmaError = 0x4,
    Complete = 0x8,
#pragma warning restore CS1591
}
