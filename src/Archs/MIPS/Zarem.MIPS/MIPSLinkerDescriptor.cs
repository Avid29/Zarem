// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Linker;
using Zarem.Linker.Config;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="ILinkerDescriptor"/> for the MIPS assembler.
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
