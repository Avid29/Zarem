// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Zarem.Services.Settings;
using Zarem.Services.Settings.Enums;
using Windows.Storage;

namespace Zarem.WinUI.Services.Settings;

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
        Local.SetValue(SettingsKeys.AppTheme, Theme.Default, false);
        Local.SetValue<string?>(SettingsKeys.LanguageOverride, null, false);
        Local.SetValue(SettingsKeys.RestoreOpenProject, true, false);

        // Editor
        Local.SetValue(SettingsKeys.RealTimeAssembly, true, false);
        Local.SetValue(SettingsKeys.AnnotationThreshold, AnnotationThreshold.Errors, false);
        Local.SetValue($"{SettingsKeys.EditorColorSchemeBase}-Dark", SettingsKeys.DefaultDarkColorScheme, false);
        Local.SetValue($"{SettingsKeys.EditorColorSchemeBase}-Light", SettingsKeys.DefaultLightColorScheme, false);

        // Assembler
        Local.SetValue<string?>(SettingsKeys.AssemblerLanguageOverride, null, false);
    }
}
