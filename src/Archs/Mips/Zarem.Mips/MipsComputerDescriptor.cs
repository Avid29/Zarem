// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Emulator.Machine;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Machine;

namespace Zarem.Mips;

/// <summary>
/// An <see cref="IComputerDescriptor"/> for the MIPS emulator.
/// </summary>
[ZaremPlugin]
public class MipsComputerDescriptor : IComputerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type ConfigType => typeof(MipsEmulatorConfig);

    /// <inheritdoc/>
    public Type ComputerType => typeof(MipsComputer);

    IComputer? IComputerDescriptor.Create(object config)
    {
        if (config is not MipsEmulatorConfig mipsConfig)
            return null;

        return new MipsComputer(mipsConfig);
    }
}
