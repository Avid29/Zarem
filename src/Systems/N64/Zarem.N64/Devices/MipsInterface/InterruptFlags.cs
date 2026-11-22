// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.MipsInterface;

/// <summary>
/// An enum for parsing the <see cref="MiRegister.Interrupt"/> or <see cref="MiRegister.InterruptMask"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/MIPS_Interface#0x0430_0008_-_MI_INTERRUPT
/// </remarks>
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
