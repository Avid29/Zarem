// Avishai Dernis 2026

using Zarem.Assembler.Config;
using Zarem.Config;
using Zarem.Emulator.Config;
using Zarem.Linker.Config;
using Zarem.Mips.Assembler;
using Zarem.Mips.Linker.Config;
using Zarem.Mips.Models.Instructions.Enums;

namespace Zarem.Mips;

/// <summary>
/// An <see cref="IArchitectureConfig"/> for the MIPS Architecture.
/// </summary>
public sealed class MipsArchitectureConfig : ConfigBase, IArchitectureConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsArchitectureConfig"/> class.
    /// </summary>
    public MipsArchitectureConfig()
    {
        Version = MipsVersion.Mips32R2;
        AssemblerConfig = new MipsAssemblerConfig();
        EmulatorConfig = new MipsEmulatorConfig();
        LinkerConfig = new MipsLinkerConfig();
    }

    /// <summary>
    /// Gets the mips version.
    /// </summary>
    public MipsVersion Version
    {
        get => field;
        init
        {
            field = value;
            AssemblerConfig?.Version = value;
            EmulatorConfig?.Version = value;
            LinkerConfig?.Version = value;
        }
    }

    /// <inheritdoc cref="IArchitectureConfig.AssemblerConfig"/>
    public MipsAssemblerConfig AssemblerConfig
    {
        get;
        init
        {
            field = value;
            value?.Version = Version;
        }
    }

    /// <inheritdoc cref="IArchitectureConfig.EmulatorConfig"/>
    public MipsEmulatorConfig EmulatorConfig
    {
        get;
        init
        {
            field = value;
            value?.Version = Version;
        }
    }

    /// <inheritdoc cref="IArchitectureConfig.LinkerConfig"/>
    public MipsLinkerConfig LinkerConfig
    {
        get;
        init
        {
            field = value;
            value?.Version = Version;
        }
    }

    AssemblerConfig IArchitectureConfig.AssemblerConfig => AssemblerConfig;

    EmulatorConfig IArchitectureConfig.EmulatorConfig => EmulatorConfig;

    LinkerConfig IArchitectureConfig.LinkerConfig => LinkerConfig;

    /// <inheritdoc/>
    public override object Clone()
    {
        return new MipsArchitectureConfig
        {
            Version = Version,
            AssemblerConfig = (MipsAssemblerConfig)AssemblerConfig.Clone(),
            EmulatorConfig = (MipsEmulatorConfig)EmulatorConfig.Clone(),
            LinkerConfig = (MipsLinkerConfig)LinkerConfig.Clone()
        };
    }
}
