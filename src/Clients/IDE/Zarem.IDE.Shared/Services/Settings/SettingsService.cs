// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Windows.Storage;
using Zarem.IDE.Services.Settings.Enums;

namespace Zarem.IDE.Services.Settings;

/// <summary>
/// An implementation of the <see cref="ISettingsService"/>
/// </summary>
public class SettingsService : ISettingsService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="messenger"></param>
    public SettingsService(IMessenger messenger)
    {
        Local = new SettingsProvider(messenger, ApplicationData.Current.LocalSettings.Values);

        EstablishDefaults();
    }

    public ISettingsProvider Local { get; }

    public Theme DefaultTheme => App.Current.RequestedTheme switch
    {
        ApplicationTheme.Dark => Theme.Dark,
        ApplicationTheme.Light => Theme.Light,
        _ => Theme.Dark,
    };

    private void EstablishDefaults()
    {
        // App
        ValidateSetting(SettingsKeys.AppTheme, Theme.Default);
        ValidateSetting<string?>(SettingsKeys.LanguageOverride, null);
        ValidateSetting(SettingsKeys.RestoreOpenProject, true);

        // Editor
        ValidateSetting(SettingsKeys.RealTimeAssembly, true);
        ValidateSetting(SettingsKeys.AnnotationThreshold, AnnotationThreshold.Errors);
        ValidateSetting($"{SettingsKeys.EditorColorSchemeBase}-Dark", SettingsKeys.DefaultDarkColorScheme);
        ValidateSetting($"{SettingsKeys.EditorColorSchemeBase}-Light", SettingsKeys.DefaultLightColorScheme);

        // Assembler
        ValidateSetting<string?>(SettingsKeys.AssemblerLanguageOverride, null);
    }

    private void ValidateSetting<T>(string key, T defaultValue)
    {
        bool isValid = true;
        try
        {
            Local.GetValue<T>(key);
        }
        catch
        {
            isValid = false;
        }

        Local.SetValue(key, defaultValue, !isValid);
    }
}
