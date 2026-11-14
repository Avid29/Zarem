// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Services;

namespace Zarem.IDE.Services;

/// <summary>
/// An implementation of the <see cref="IThemeService"/>
/// </summary>
public class ThemeService : IThemeService
{
    public Theme DefaultTheme => App.Current.RequestedTheme switch
    {
        ApplicationTheme.Dark => Theme.Dark,
        ApplicationTheme.Light => Theme.Light,
        _ => Theme.Dark,
    };
}
