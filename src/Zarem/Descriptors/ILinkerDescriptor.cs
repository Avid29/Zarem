// Avishai Dernis 2026

using System;
using Zarem.Descriptors.Base;

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing a linker.
/// </summary>
public interface ILinkerDescriptor : IConfigDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the linker's architecture handler.
    /// </summary>
    Type LinkerHandlerType { get; }
}
