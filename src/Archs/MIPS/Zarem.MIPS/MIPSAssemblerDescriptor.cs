// Avishai Dernis 2026

using System;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Descriptors;
using Zarem.Attributes;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="IAssemblerDescriptor"/> for the MIPS assembler.
/// </summary>
[ZaremPlugin]
public class MIPSAssemblerDescriptor : IAssemblerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type AssemblerHandlerType => typeof(MipsAssmblerHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(MipsAssemblerConfig);
}
