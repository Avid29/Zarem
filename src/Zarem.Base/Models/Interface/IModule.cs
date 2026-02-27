// Avishai Dernis 2026

namespace Zarem.Models.Interface;

/// <summary>
/// An interface for a concrete object modelule.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the file path of the <see cref="IModule"/>.
    /// </summary>
    string? Identity { get; }
}
