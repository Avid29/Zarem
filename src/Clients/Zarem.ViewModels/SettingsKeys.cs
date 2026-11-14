// Avishai Dernis 2025

using Zarem.IDE.Services.Settings.Enums;
using Zarem.Services;
using Zarem.Services.Settings;

namespace Zarem;

/// <summary>
/// A collection of keys used for settings.
/// </summary>
public static class SettingsKeys
{
#pragma warning disable CS1591
    // App
    public const string AppTheme = nameof(AppTheme);
    public const string LanguageOverride = nameof(LanguageOverride);
#pragma warning restore CS1591

    /// <summary>
    /// Gets the current theme as a string, either "Light" or "Dark".
    /// </summary>
    public static string CurrentTheme
    {
        get
        {
            var theme = SettingsService.Local.GetValue<Theme>(AppTheme);
            if (theme is Theme.Default)
            {
                theme = ThemeService.DefaultTheme;
            }

            return theme switch
            {
                Theme.Light => "Light",
                Theme.Dark or _ => "Dark",
            };
        }
    }

    private static ISettingsService SettingsService => Service.Get<ISettingsService>();

    private static IThemeService ThemeService => Service.Get<IThemeService>();
}
