// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.RiscV.Linker;
using Zarem.RiscV.Linker.Config;

namespace Zarem.RiscV;

/// <summary>
/// An <see cref="ILinkerDescriptor"/> for the RISC-V linker.
/// </summary>
[ZaremPlugin]
public class RiscVLinkerDescriptor : ILinkerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "RISC-V";

    /// <inheritdoc/>
    public Type LinkerHandlerType => typeof(RiscVLinkerHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(RiscVLinkerConfig);
}
