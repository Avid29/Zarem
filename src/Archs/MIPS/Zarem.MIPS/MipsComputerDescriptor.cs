// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="IComputerDescriptor"/> for the MIPS emulator.
/// </summary>
[ZaremPlugin]
public class MipsComputerDescriptor : IComputerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type ConfigType => typeof(MIPSEmulatorConfig);

    /// <inheritdoc/>
    public Type ComputerType => typeof(MipsComputer);

    IComputer? IComputerDescriptor.Create(object config)
    {
        if (config is not MIPSEmulatorConfig mipsConfig)
            return null;

        return new MipsComputer(mipsConfig);
    }
}
