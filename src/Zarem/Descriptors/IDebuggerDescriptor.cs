// Avishai Dernis 2026

using System;
using Zarem.Debugger.Handlers;

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing an <see cref="IDebugHandler"/>
/// </summary>
public interface IDebuggerDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the <see cref="IDebugHandler"/> for an architecture.
    /// </summary>
    Type DebugHandleType { get; }
}
