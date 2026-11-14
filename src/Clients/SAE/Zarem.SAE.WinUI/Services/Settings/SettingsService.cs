// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using Windows.Storage;
using Zarem.Services.Settings;

namespace Zarem.SAE.Services.Settings;

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

    private void EstablishDefaults()
    {
        ValidateSetting<string?>(SettingsKeys.BiosRomPath, null);
        ValidateSetting<string?>(SettingsKeys.CartridgeRomPath, null);
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
