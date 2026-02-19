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
    string? FilePath { get; }

    /// <summary>
    /// Gets the file name of the <see cref="IModule"/>.
    /// </summary>
    string? FileName { get; }

    /// <summary>
    /// Gets the display name of the module.
    /// </summary>
    string DisplayName { get; }
}
