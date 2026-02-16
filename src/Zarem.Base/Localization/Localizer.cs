// Avishai Dernis 2025

using System.Reflection;
using System.Resources;

namespace Zarem.Localization;

/// <summary>
/// A base class for a <see cref="IStringLocalizer"/> that uses a <see cref="ResourceManager"/> to load resources.
/// </summary>
public class Localizer : IStringLocalizer
{
    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="Localizer"/> class.
    /// </summary>
    public Localizer(string @namespace, Assembly assembly)
    {
        _resourceManager = new ResourceManager(@namespace, assembly);
    }

    /// <inheritdoc/>
    public string? Namespace => _resourceManager.BaseName;

    /// <summary>
    /// Gets the localized string for the given key.
    /// </summary>
    public string? this[string key, params object?[] args]
    {
        get
        {
            var localized = _resourceManager.GetString(key);
            if (!string.IsNullOrEmpty(localized))
                return localized;

            return null;
        }
    }
}
