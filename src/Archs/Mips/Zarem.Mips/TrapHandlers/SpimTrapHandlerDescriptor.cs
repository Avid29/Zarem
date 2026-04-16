// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Emulator.TrapHandlers;

namespace Zarem.MIPS.TrapHandlers;

/// <summary>
/// An <see cref="ITrapHandlerDescriptor"/> for the <see cref="SpimTrapHandler"/>.
/// </summary>
[ZaremPlugin]
public class SpimTrapHandlerDescriptor : ITrapHandlerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "SPIM";

    /// <inheritdoc/>
    public Type Type => typeof(SpimTrapHandler);
}
