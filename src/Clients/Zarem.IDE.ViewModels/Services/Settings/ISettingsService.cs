// Avishai Dernis 2025

using Zarem.IDE.Services.Settings.Enums;

namespace Zarem.IDE.Services.Settings;

/// <summary>
/// An interface for a service that handles loading and storing app settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the <see cref="ISettingsProvider"/> for the local settings folder.
    /// </summary>
    ISettingsProvider Local { get; }

    /// <summary>
    /// Gets the default theme.
    /// </summary>
    Theme DefaultTheme { get;  }
}
