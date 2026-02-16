// Avishai Dernis 2026

using System;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Registry.Attributes;
using Zarem.Registry.Descriptors;

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
    public Type AssemblerHandlerType => typeof(MIPSHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(MIPSAssemblerConfig);
}
