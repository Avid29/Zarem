// Avishai Dernis 2026

using Zarem.Assembler.Config;
using Zarem.Config;
using Zarem.Emulator.Config;
using Zarem.Linker.Config;
using Zarem.Models.Instructions.Enums;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="IArchitectureConfig"/> for the MIPS Architecture.
/// </summary>
public sealed class MIPSArchitectureConfig : IArchitectureConfig
{
    /// <summary>
    /// Gets the mips version 
    /// </summary>
    public MipsVersion MipsVersion
    {
        get => field;
        set
        {
            field = value;

            AssemblerConfig?.MipsVersion = value;
            EmulatorConfig?.MipsVersion = value;
        }
    }

    /// <inheritdoc cref="IArchitectureConfig.AssemblerConfig"/>
    public MIPSAssemblerConfig? AssemblerConfig { get; init; }

    /// <inheritdoc cref="IArchitectureConfig.EmulatorConfig"/>
    public MIPSEmulatorConfig? EmulatorConfig { get; init; }

    /// <inheritdoc cref="IArchitectureConfig.LinkerConfig"/>
    public MIPSLinkerConfig? LinkerConfig { get; init; }

    AssemblerConfig? IArchitectureConfig.AssemblerConfig => AssemblerConfig;

    EmulatorConfig? IArchitectureConfig.EmulatorConfig => EmulatorConfig;

    LinkerConfig? IArchitectureConfig.LinkerConfig => LinkerConfig;
}
