// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Emulator.TrapHandlers;

namespace Zarem.MIPS.TrapHandlers;

/// <summary>
/// An <see cref="ITrapHandlerDescriptor"/> for the Zarem MIPS trap handler.
/// </summary>
[ZaremPlugin]
public class ZaremTrapHandlerDescriptor : ITrapHandlerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "Zarem";

    /// <inheritdoc/>
    public Type Type => typeof(ZaremTrapHandler);
}
