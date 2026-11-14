// Avishai Dernis 2024

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.StartScreen;
using Zarem.IDE.Helpers;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Settings;
using Zarem.IDE.ViewModels;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.CheatSheet;
using Zarem.IDE.ViewModels.Pages.Settings;
using Zarem.IDE.Windows;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Popup;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.Services.Windowing;
using Zarem.ViewModels.Pages;

namespace Zarem.IDE;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class ZaremIDEApp : ZaremAppBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZaremIDEApp"/> class.
    /// </summary>
    public ZaremIDEApp()
    {
        this.InitializeComponent();

        // Setup jump list config
        _ = SetupJumpList();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Window = WindowHelper.CreateWindow<MainWindow>();
        Window.Activate();

        // Pass the launch arguments to the base class for further processing
        base.OnLaunched(args);

# if !HAS_UNO
        // Handle activiation arguments
        var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        bool success = activationArgs.Kind switch
        {
            ExtendedActivationKind.File => await HandleFileArgAsync(activationArgs),
            _ => false,
        };
#endif
    }

    /// <inheritdoc/>
    protected override ServiceProvider ConfigureServices() =>
        new ServiceCollection()

            // Basic Services
            .AddSingleton<ICacheService, CacheService>()
            .AddSingleton<IClipboardService, ClipboardService>()
            .AddSingleton<IConsoleService, ConsoleService>()
            .AddSingleton<IDispatcherService, DispatcherService>()
            .AddSingleton<ILocalizationService, LocalizationService>()
            .AddSingleton<IMessenger, WeakReferenceMessenger>()
            .AddSingleton<IThemeService, ThemeService>()
            .AddSingleton<IVersioningService, VersioningService>()
            .AddSingleton<IWindowingService, WindowingService>()

            // Dependent Services
            .AddSingleton<IPopupService, PopupService>()
            .AddSingleton<IStateService, StateService>()
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
            .AddSingleton<RegisterViewerViewModel>()
            .AddSingleton<GraphicalOutputPageViewModel>()

            // ViewModels
            .AddSingleton<StatusViewModel>()
            .AddTransient<WindowViewModel>()
            .AddTransient<PanelViewModel>()
            .AddSingleton<MainViewModel>()
            .BuildServiceProvider();

    private static async Task SetupJumpList()
    {
#if !HAS_UNO
        var jumpList = await JumpList.LoadCurrentAsync();
        jumpList.SystemGroupKind = JumpListSystemGroupKind.Recent;
        await jumpList.SaveAsync();
#endif
    }

    private static async Task<bool> HandleFileArgAsync(AppActivationArguments args)
    {

#if !HAS_UNO
        if (args.Data is not IFileActivatedEventArgs fileArgs)
            return false;

        if (fileArgs.Files.Count is <= 0)
            return false;

        var file = fileArgs.Files[0];

        switch (Path.GetExtension(file.Path))
        {
            case ".asm":
                var fileService = Service.Get<IFileService>();
                var bFile = await fileService.GetFileAsync(file.Path);
                bFile?.Open();
                break;
            case ".zrmp":
                var projectService = Service.Get<IProjectService>();
                await projectService.OpenProjectAsync(file.Path);
                break;
        }
#endif

        return true;
    }
}
