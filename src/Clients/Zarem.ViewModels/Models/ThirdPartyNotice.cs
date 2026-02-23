// Avishai Dernis 2025

namespace Zarem.Models;

/// <summary>
/// A class for containing the compoenents of a dependencies details.
/// </summary>
public class ThirdPartyNotice(string dependencyName, string url)
{
    /// <summary>
    /// Gets the name of the dependency.
    /// </summary>
    public string DependencyName { get; init; } = dependencyName;

    /// <summary>
    /// Gets the url link to the dependency.
    /// </summary>
    public string Url { get; init; } = url;
}
