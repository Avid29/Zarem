// Avishai Dernis 2026

using Zarem.Assembler.Config;
using Zarem.Config;
using Zarem.Emulator.Config;
using Zarem.Linker.Config;
using Zarem.Models.Versioning;
using Zarem.RiscV.Assembler;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Linker.Config;

namespace Zarem.RiscV;

/// <summary>
/// An <see cref="IArchitectureConfig"/> for the RISC-V Architecture.
/// </summary>
public sealed class RiscVArchitectureConfig : ConfigBase, IArchitectureConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVArchitectureConfig"/> class.
    /// </summary>
    public RiscVArchitectureConfig()
    {
        VersionInfo = new RiscVVersionInfo();
        AssemblerConfig = new RiscVAssemblerConfig();
        EmulatorConfig = new RiscVEmulatorConfig();
        LinkerConfig = new RiscVLinkerConfig();
    }

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
    public RiscVAssemblerConfig AssemblerConfig
    {
        get => field;
        set
        {
            field = value;
            value.VersionInfo = VersionInfo;
        }
    }

    /// <inheritdoc cref="IArchitectureConfig.EmulatorConfig"/>
    public RiscVEmulatorConfig EmulatorConfig
    {
        get => field;
        set
        {
            field = value;
            value.VersionInfo = VersionInfo;
        }
    }

    /// <inheritdoc cref="IArchitectureConfig.LinkerConfig"/>
    public RiscVLinkerConfig LinkerConfig
    {
        get => field;
        set
        {
            field = value;
            value.VersionInfo = VersionInfo;
        }
    }

    AssemblerConfig IArchitectureConfig.AssemblerConfig => AssemblerConfig;

    EmulatorConfig IArchitectureConfig.EmulatorConfig => EmulatorConfig;

    LinkerConfig IArchitectureConfig.LinkerConfig => LinkerConfig;

    /// <inheritdoc/>
    public override object Clone()
    {
        return new RiscVArchitectureConfig
        {
            VersionInfo = VersionInfo,
            AssemblerConfig = (RiscVAssemblerConfig)AssemblerConfig.Clone(),
            EmulatorConfig = (RiscVEmulatorConfig)EmulatorConfig.Clone(),
            LinkerConfig = (RiscVLinkerConfig)LinkerConfig.Clone()
        };
    }
}
