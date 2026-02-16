// Avishai Dernis 2026

using System;
using Zarem.Descriptors;

namespace Zarem.Registry.Descriptors;

/// <summary>
/// An interface for a class describing a supported architecture.
/// </summary>
public interface IModuleFormatDescriptor : IDisplayDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the format.
    /// </summary>
    Type FormatType { get; }
}
