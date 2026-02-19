// Adam Dernis 2024

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Popup;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.Services.Windowing;
using Zarem.ViewModels;
using Zarem.ViewModels.Pages;
using Zarem.ViewModels.Pages.App;
using Zarem.WinUI.Services;
using Zarem.WinUI.Services.Popup;
using Zarem.WinUI.Services.Settings;
using Zarem.WinUI.Services.Versioning;
using Zarem.WinUI.Services.Windowing;
using System;
using Zarem.WinUI.Services.Files;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

namespace Zarem.WinUI;

public partial class App
{
    private static IServiceProvider ConfigureServices()
    {
        // Register services
        return new ServiceCollection()

            // Basic Services
            .AddSingleton<ICacheService, CacheService>()
            .AddSingleton<IClipboardService, ClipboardService>()
            .AddSingleton<IConsoleService, ConsoleService>()
            .AddSingleton<IDispatcherService, DispatcherService>()
            .AddSingleton<ILocalizationService, LocalizationService>()
            .AddSingleton<IMessenger, WeakReferenceMessenger>()
            .AddSingleton<IPopupService, PopupService>()
            .AddSingleton<IVersioningService, VersioningService>()
            .AddSingleton<IWindowingService, WindowingService>()

            // Dependent Services
            .AddSingleton<IFileSystemService, FileSystemService>()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddSingleton<IProjectService, ProjectService>()
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IBuildService, BuildService>()
            .AddSingleton<IDebugService, DebugService>()

            // Page ViewModels
            .AddTransient<AboutPageViewModel>()
            .AddTransient<CreateProjectViewModel>()
            .AddTransient<CheatSheetViewModel>()
            .AddTransient<FilePageViewModel>()
            .AddTransient<SettingsPageViewModel>()
            .AddTransient<WelcomePageViewModel>()

            // Panel ViewModels
            .AddSingleton<ExplorerViewModel>()
            .AddSingleton<ErrorListViewModel>()

            // ViewModels
            .AddTransient<StatusViewModel>()
            .AddTransient<WindowViewModel>()
            .AddTransient<PanelViewModel>()
            .AddSingleton<MainViewModel>()
            .BuildServiceProvider();
    }
}
