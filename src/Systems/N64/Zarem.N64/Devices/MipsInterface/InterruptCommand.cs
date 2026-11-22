// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.MipsInterface;

/// <summary>
/// An enum for parsing a command written to the <see cref="MiRegister.InterruptMask"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/MIPS_Interface#0x0430_000C_-_MI_MASK
/// </remarks>
[Flags]
public enum InterruptCommand : uint
{
#pragma warning disable CS1591
    ClearRealitySignalProcessor = 0x1,
    SetRealitySignalProcessor = 0x2,
    ClearSerialInterface = 0x4,
    SetSerialInterface = 0x8,
    ClearAudioInterace = 0x10,
    SetAudioInterace = 0x20,
    ClearVideoInterface = 0x40,
    SetVideoInterface = 0x80,
    ClearParallelInterface = 0x100,
    SetParallelInterface = 0x200,
    ClearRealityDisplayProcessor = 0x400,
    SetRealityDisplayProcessor = 0x800,
#pragma warning restore CS1591
}
