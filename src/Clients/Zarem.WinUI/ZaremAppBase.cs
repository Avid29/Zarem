// Avishai Dernis 2026

using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Messages;
using Zarem.Services;
using Zarem.Services.Settings;

namespace Zarem;

/// <summary>
/// A base class for the Zarem application, providing common functionality and behavior for all Zarem applications.
/// </summary>
public abstract partial class ZaremAppBase : Application
{
    private readonly IMessenger _messenger;
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZaremAppBase"/> class.
    /// </summary>
    public ZaremAppBase()
    {
        Services = ConfigureServices();
        Ioc.Default.ConfigureServices(Services);

        _messenger = Service.Get<IMessenger>();
        _settingsService = Service.Get<ISettingsService>();

        // Apply language override from settings
        var localizationService = Service.Get<ILocalizationService>();
        localizationService.LanguageOverride = _settingsService.Local.GetValue<string>(SettingsKeys.LanguageOverride);

        // Subscribe to messages
        _messenger.Register<ZaremAppBase, SettingChangedMessage<Theme>>(this, (r, m) => r.ApplyRequestedTheme(m.NewValue));
    }

    /// <summary>
    /// Gets the current <see cref="ZaremAppBase"/> instance in use.
    /// </summary>
    public static new ZaremAppBase Current => (ZaremAppBase)Application.Current;

    /// <summary>
    /// Gets the app current window.
    /// </summary>
    public Window? Window { get; protected set; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services required by the application. This method must be implemented by derived classes to provide specific service configurations.
    /// </summary>
    protected abstract ServiceProvider ConfigureServices();

    /// <summary>
    /// Loads the requested theme from settings and applies it.
    /// </summary>
    protected void LoadRequestedTheme()
    {
        var theme = _settingsService.Local.GetValue<Theme>(SettingsKeys.AppTheme);
        ApplyRequestedTheme(theme);
    }

    private void ApplyRequestedTheme(Theme theme = default)
    {
#if !HAS_UNO
        // Find root item
        var root = Window?.Content.FindDescendantOrSelf<FrameworkElement>();
        if (root is null)
            return;

        // Update theme from root content
        root.RequestedTheme = theme switch
        {
            Theme.Dark => ElementTheme.Dark,
            Theme.Light => ElementTheme.Light,
            Theme.Default or _ => ElementTheme.Default,
        };
#endif
    }
}
