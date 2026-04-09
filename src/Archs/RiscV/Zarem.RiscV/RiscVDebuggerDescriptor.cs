// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;

namespace Zarem.RiscV;

/// <summary>
/// An <see cref="IDebuggerDescriptor"/> for the RISC-V debugger.
/// </summary>
[ZaremPlugin]
public class RiscVDebuggerDescriptor : IDebuggerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "RISC-V";

    /// <inheritdoc/>
    public Type DebugHandleType => throw new NotImplementedException();
}
