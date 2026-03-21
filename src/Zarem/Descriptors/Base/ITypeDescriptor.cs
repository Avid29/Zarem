// Avishai Dernis 2026

using System;

namespace Zarem.Descriptors.Base;

/// <summary>
/// A shared interface for a descriptor of a zarem plugin descriptor with no config options.
/// </summary>
public interface ITypeDescriptor : IDescriptor
{
    /// <summary>
    /// Gets the type described.
    /// </summary>
    public Type Type { get; }
}
