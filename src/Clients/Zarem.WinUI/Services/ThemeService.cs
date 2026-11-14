// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Zarem.IDE.Services.Settings.Enums;

namespace Zarem.Services;

/// <summary>
/// An implementation of the <see cref="IThemeService"/>
/// </summary>
public class ThemeService : IThemeService
{
    /// <inheritdoc/>
    public Theme DefaultTheme => Application.Current.RequestedTheme switch
    {
        ApplicationTheme.Dark => Theme.Dark,
        ApplicationTheme.Light => Theme.Light,
        _ => Theme.Dark,
    };
}
