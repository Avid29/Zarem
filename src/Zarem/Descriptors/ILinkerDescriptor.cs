// Avishai Dernis 2026

using System;

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing a linker.
/// </summary>
public interface ILinkerDescriptor : IDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the linker's architecture handler.
    /// </summary>
    Type LinkerHandlerType { get; }
}
