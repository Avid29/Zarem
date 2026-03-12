// Avishai Dernis 2026

using System;
using Zarem.Attributes;
using Zarem.Debugger.MIPS;
using Zarem.Descriptors;

namespace Zarem.MIPS;

/// <summary>
/// An <see cref="IDebuggerDescriptor"/> for the MIPS debugger.
/// </summary>
[ZaremPlugin]
public class MipsDebuggerDescriptor : IDebuggerDescriptor
{
    /// <inheritdoc/>
    public Type DebugHandleType => typeof(MipsDebugHandler);
}
