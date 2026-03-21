// Avishai Dernis 2026

namespace Zarem.Descriptors.Base;

/// <summary>
/// A shared interface for a descriptor of a zarem plugin descriptor.
/// </summary>
public interface IDescriptor
{
    /// <summary>
    /// Gets the name used to identify the described type. 
    /// </summary>
    string Identifier { get; }
}
