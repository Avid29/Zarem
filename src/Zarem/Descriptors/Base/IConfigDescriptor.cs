// Avishai Dernis 2026

using System;

namespace Zarem.Descriptors.Base;

/// <summary>
/// A shared interface for a descriptor of a zarem plugin descriptor with a config.
/// </summary>
public interface IConfigDescriptor : IDescriptor
{
    /// <summary>
    /// Gets the type of the configuration associated with the descriptor.
    /// </summary>
    Type ConfigType { get; }
}
