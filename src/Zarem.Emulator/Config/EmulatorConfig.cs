// Avishai Dernis 2026

using System;
using Zarem.Config;
using Zarem.Emulator.TrapHandlers;

namespace Zarem.Emulator.Config;

/// <summary>
/// A class containing emulator configurations.
/// </summary>
public class EmulatorConfig : IConfig
{
    /// <summary>
    /// Gets or sets the emulator's trap handler.
    /// </summary>
    /// <remarks>
    /// If null, all traps EXCEPT BREAK will be handled by the host-layer, and not the emulated machine.
    /// If not null, the trap handler will interpret traps and syscalls.
    /// </remarks>
    public ITrapHandler? TrapHost { get; init; }
}
