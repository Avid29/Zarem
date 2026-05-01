// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.N64.Config;

namespace Zarem.N64;

/// <summary>
/// An <see cref="IComputerDescriptor"/> for the N64 emulator.
/// </summary>
[ZaremPlugin]
public class N64ComputerDescriptor : IComputerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "N64";

    /// <inheritdoc/>
    public Type ConfigType => typeof(N64EmulatorConfig);

    /// <inheritdoc/>
    public Type ComputerType => typeof(Nintendo64);

    IComputer? IComputerDescriptor.Create(object config)
    {
        if (config is not N64EmulatorConfig n64Config)
            return null;

        return new Nintendo64(n64Config);
    }
}
