// Avishai Dernis 2026

using Zarem.Assembler.Config;
using Zarem.Emulator.Config;
using Zarem.Linker.Config;

namespace Zarem.Config;

/// <summary>
/// An interface for an architecture's configuration.
/// </summary>
public interface IArchitectureConfig : IConfig
{
    /// <summary>
    /// Gets the assembler configuration.
    /// </summary>
    AssemblerConfig AssemblerConfig { get; }

    /// <summary>
    /// Gets the linker config.
    /// </summary>
    LinkerConfig LinkerConfig { get; }

    /// <summary>
    /// Gets the emulator config.
    /// </summary>
    EmulatorConfig EmulatorConfig { get; }
}
