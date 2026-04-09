// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.RiscV;

/// <summary>
/// An <see cref="IComputerDescriptor"/> for the RISC-V emulator.
/// </summary>
[ZaremPlugin]
public class RiscVComputerDescriptor : IComputerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "RISC-V";

    /// <inheritdoc/>
    public Type ConfigType => typeof(RiscVEmulatorConfig);

    /// <inheritdoc/>
    public Type ComputerType => typeof(RiscVComputer);

    IComputer? IComputerDescriptor.Create(object config)
    {
        if (config is not RiscVEmulatorConfig mipsConfig)
            return null;

        return new RiscVComputer(mipsConfig);
    }
}
