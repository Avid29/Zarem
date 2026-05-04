// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.RiscV.Assembler;

namespace Zarem.RiscV;

/// <summary>
/// An <see cref="IAssemblerDescriptor"/> for the RISC-V assembler.
/// </summary>
[ZaremPlugin]
public class RiscVAssemblerDescriptor : IAssemblerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "RISC-V";

    /// <inheritdoc/>
    public Type AssemblerHandlerType => typeof(RiscVAssemblerHandler);

    /// <inheritdoc/>
    public Type ConfigType => typeof(RiscVAssemblerConfig);
}
