// Avishai Dernis 2026

using Zarem.Emulator;
using Zarem.Emulator.Config;

namespace Zarem.Components.Interfaces;

/// <summary>
/// An interface for a component of a <see cref="Project"/> that emulates machines.
/// </summary>
public interface IEmulateComponent : IProjectComponent
{
    /// <summary>
    /// Gets the emulator config.
    /// </summary>
    EmulatorConfig Config { get; }

    /// <summary>
    /// Creates a new emulator.
    /// </summary>
    IEmulator? CreateEmulator();
}
