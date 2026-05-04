// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Mips.Assembler;

namespace Zarem.Mips;

/// <summary>
/// An <see cref="IAssemblerDescriptor"/> for the MIPS assembler.
/// </summary>
[ZaremPlugin]
public class MipsAssemblerDescriptor : IAssemblerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type AssemblerHandlerType => typeof(MipsAssemblerHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(MipsAssemblerConfig);
}
