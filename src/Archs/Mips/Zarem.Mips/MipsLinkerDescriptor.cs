// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Mips.Linker;
using Zarem.Mips.Linker.Config;

namespace Zarem.Mips;

/// <summary>
/// An <see cref="ILinkerDescriptor"/> for the MIPS linker.
/// </summary>
[ZaremPlugin]
public class MipsLinkerDescriptor : ILinkerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type LinkerHandlerType => typeof(MipsLinkerHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(MipsLinkerConfig);
}
