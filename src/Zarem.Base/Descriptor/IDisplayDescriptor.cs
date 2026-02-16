// Avishai Dernis 2026


// Avishai Dernis 2026

namespace Zarem.Descriptors;

/// <summary>
/// A shared interface for a descriptor of a zarem plugin descriptor with display info.
/// </summary>
public interface IDisplayDescriptor : IDescriptor
{
    /// <summary>
    /// Gets the name used to display the described type.
    /// </summary>
    string? DisplayName { get; }
}
