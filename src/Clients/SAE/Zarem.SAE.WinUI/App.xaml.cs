// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Zarem.SAE.Services;
using Zarem.SAE.Services.Settings;
using Zarem.SAE.ViewModels;
using Zarem.SAE.Windows;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Popup;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.Services.Windowing;

namespace Zarem.SAE;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class ZaremSAEApp : ZaremAppBase
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public ZaremSAEApp()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        Window.Activate();

        // Pass the launch arguments to the base class for further processing
        base.OnLaunched(args);
    }

    protected override ServiceProvider ConfigureServices() =>
        new ServiceCollection()

            // Basic Services
            .AddSingleton<IClipboardService, ClipboardService>()
            .AddSingleton<IDispatcherService, DispatcherService>()
            .AddSingleton<ILocalizationService, LocalizationService>()
            .AddSingleton<IMessenger, WeakReferenceMessenger>()
            .AddSingleton<IThemeService, ThemeService>()
            .AddSingleton<IVersioningService, VersioningService>()
            .AddSingleton<IWindowingService, WindowingService>()

            // Dependent Services
            .AddSingleton<IPopupService, PopupService>()
            .AddSingleton<IFileSystemService, FileSystemService>()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddSingleton<IEmulatorService, EmulatorService>()

            // ViewModels
            .AddTransient<WindowViewModel>()
            .AddSingleton<MainViewModel>()
            .BuildServiceProvider();
}
