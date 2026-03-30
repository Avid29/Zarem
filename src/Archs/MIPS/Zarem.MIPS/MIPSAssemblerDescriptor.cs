// Avishai Dernis 2026

using System;
using Zarem.Descriptors;
using Zarem.Attributes;
using Zarem.Assembler;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="IAssemblerDescriptor"/> for the MIPS assembler.
/// </summary>
[ZaremPlugin]
public class MipsAssemblerDescriptor : IAssemblerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type AssemblerHandlerType => typeof(MipsAssmblerHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(MipsAssemblerConfig);
}
