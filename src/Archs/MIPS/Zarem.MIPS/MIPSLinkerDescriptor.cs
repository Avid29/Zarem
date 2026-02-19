// Avishai Dernis 2026

using System;
using Zarem.Assembler.Handlers;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Linker.Config;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="ILinkerDescriptor"/> for the MIPS assembler.
/// </summary>
[ZaremPlugin]
public class MIPSLinkerDescriptor : ILinkerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type LinkerHandlerType => typeof(MIPSAssmblerHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(MIPSLinkerConfig);
}
