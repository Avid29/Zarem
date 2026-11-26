// Avishai Dernis 2026

using Zarem.Services.Settings.Enums;

namespace Zarem.Services;

/// <summary>
/// An interface for getting application theme information.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the default theme.
    /// </summary>
    Theme DefaultTheme { get; }
}
