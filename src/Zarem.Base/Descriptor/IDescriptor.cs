// Avishai Dernis 2026

using System;

namespace Zarem.Descriptors;

/// <summary>
/// A shared interface for a descriptor of a zarem plugin descriptor.
/// </summary>
public interface IDescriptor
{
    /// <summary>
    /// Gets the name used to identify the described type. 
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// Gets the type of the configuration associated with the descriptor.
    /// </summary>
    Type ConfigType { get; }
}
