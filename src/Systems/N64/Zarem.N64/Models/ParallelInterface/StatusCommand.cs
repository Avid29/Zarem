// Avishai Dernis 2026

using System;
using Zarem.N64.Models.Enums.Registers;

namespace Zarem.N64.Models.ParallelInterface;

/// <summary>
/// An enum for parsing a command written to the <see cref="PiRegister.Status"/> register.
/// </summary>
[Flags]
public enum StatusCommand : uint
{
#pragma warning disable CS1591
    Reset = 0x1,
    ClearInterrupt = 0x2,
#pragma warning restore CS1591
}
