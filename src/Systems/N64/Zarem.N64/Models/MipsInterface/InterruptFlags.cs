// Avishai Dernis 2026

using System;
using Zarem.N64.Models.Enums.Registers;

namespace Zarem.N64.Models.MipsInterface;

/// <summary>
/// An enum for parsing the <see cref="MiRegister.Interrupt"/> or <see cref="MiRegister.InterruptMask"/> register.
/// </summary>
[Flags]
public enum InterruptFlags : uint
{
#pragma warning disable CS1591
    RealitySignalProcessor = 0x1,
    SerialInterface = 0x2,
    AudioInterace = 0x4,
    VideoInterface = 0x8,
    ParallelInterface = 0x10,
    RealityDisplayProcessor = 0x20,
#pragma warning restore CS1591
}
