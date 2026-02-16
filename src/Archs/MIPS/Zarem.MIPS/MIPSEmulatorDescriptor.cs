// Avishai Dernis 2026

using System;
using Zarem.Descriptors;
using Zarem.Emulator;
using Zarem.Emulator.Config;
using Zarem.Attributes;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="IEmulatorDescriptor"/> for the MIPS emulator.
/// </summary>
[ZaremPlugin]
public class MIPSEmulatorDescriptor : IEmulatorDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type ConfigType => typeof(MIPSEmulatorConfig);

    /// <inheritdoc/>
    public Type EmulatorType => typeof(MIPSEmulator);

    /// <inheritdoc cref="IEmulatorDescriptor.Create(object)"/>
    public MIPSEmulator Create(MIPSEmulatorConfig config) => new(config);

    IEmulator? IEmulatorDescriptor.Create(object config)
    {
        if (config is not MIPSEmulatorConfig mipsConfig)
            return null;

        return Create(mipsConfig);
    }
}
