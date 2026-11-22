// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// A flagged enum for the <see cref="BankStatus.BankValid"/> and <see cref="BankStatus.BankDirty"/> fields.
/// </summary>
[Flags]
public enum BankFlags : byte
{
#pragma warning disable CS1591
    Bank0 = 0x01,
    Bank1 = 0x02,
    Bank2 = 0x04,
    Bank3 = 0x08,
    Bank4 = 0x10,
    Bank5 = 0x20,
    Bank6 = 0x40,
    Bank7 = 0x80,
#pragma warning restore CS1591
}
