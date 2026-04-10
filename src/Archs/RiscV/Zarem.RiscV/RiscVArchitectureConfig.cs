// Avishai Dernis 2026

using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Config;
using Zarem.Emulator.Config;
using Zarem.Linker.Config;
using Zarem.Models.Versioning;

namespace Zarem.RiscV;

/// <summary>
/// An <see cref="IArchitectureConfig"/> for the RISC-V Architecture.
/// </summary>
public sealed class RiscVArchitectureConfig : IArchitectureConfig
{
    /// <summary>
    /// Gets the RISC-V Version info.
    /// </summary>
    public RiscVVersionInfo VersionInfo
    {
        get => field;
        set
        {
            field = value;

            AssemblerConfig?.VersionInfo = value;
            EmulatorConfig?.VersionInfo = value;
            LinkerConfig?.VersionInfo = value;
        }
    }

    /// <inheritdoc cref="IArchitectureConfig.AssemblerConfig"/>
    public RiscVAssemblerConfig? AssemblerConfig { get; init; }

    /// <inheritdoc cref="IArchitectureConfig.EmulatorConfig"/>
    public RiscVEmulatorConfig? EmulatorConfig { get; init; }

    /// <inheritdoc cref="IArchitectureConfig.LinkerConfig"/>
    public RiscVLinkerConfig? LinkerConfig { get; init; }

    AssemblerConfig? IArchitectureConfig.AssemblerConfig => AssemblerConfig;

    EmulatorConfig? IArchitectureConfig.EmulatorConfig => EmulatorConfig;

    LinkerConfig? IArchitectureConfig.LinkerConfig => LinkerConfig;
}
