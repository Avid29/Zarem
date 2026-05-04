// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Descriptors;
using Zarem.Mips.Debugger;

namespace Zarem.Mips;

/// <summary>
/// An <see cref="IDebuggerDescriptor"/> for the MIPS debugger.
/// </summary>
[ZaremPlugin]
public class MipsDebuggerDescriptor : IDebuggerDescriptor
{
    /// <inheritdoc/>
    public string Identifier => "MIPS";

    /// <inheritdoc/>
    public Type DebugHandleType => typeof(MipsDebugHandler);
}
