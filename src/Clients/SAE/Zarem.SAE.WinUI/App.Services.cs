// Avishai Dernis 2024

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Zarem.IDE.Services.Popup;
using Zarem.SAE.Services;
using Zarem.SAE.Services.Files;
using Zarem.SAE.Services.Popup;
using Zarem.SAE.Services.Settings;
using Zarem.SAE.Services.Versioning;
using Zarem.SAE.ViewModels;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

namespace Zarem.SAE;

public partial class App
{
    private static ServiceProvider ConfigureServices() =>
        new ServiceCollection()

            // Basic Services
            .AddSingleton<IClipboardService, ClipboardService>()
            .AddSingleton<IDispatcherService, DispatcherService>()
            .AddSingleton<ILocalizationService, LocalizationService>()
            .AddSingleton<IMessenger, WeakReferenceMessenger>()
            .AddSingleton<IThemeService, ThemeService>()
            .AddSingleton<IVersioningService, VersioningService>()

            // Dependent Services
            .AddSingleton<IPopupService, PopupService>()
            .AddSingleton<IFileSystemService, FileSystemService>()
            .AddSingleton<ISettingsService, SettingsService>()

            // ViewModels
            .AddSingleton<MainViewModel>()
            .BuildServiceProvider();
}
