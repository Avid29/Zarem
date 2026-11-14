// Avishai Dernis 2025

using Zarem.Services.Versioning.Models;

namespace Zarem.Services.Versioning;

/// <summary>
/// An <see langword="interface"/> for a service that retrieves version info about the app.
/// </summary>
public interface IVersioningService
{
    /// <summary>
    /// Gets the app's version info.
    /// </summary>
    public AppVersion AppVersion { get; }

    /// <summary>
    /// Gets the git version info.
    /// </summary>
    public GitVersionInfo GitVersionInfo { get; }
}
