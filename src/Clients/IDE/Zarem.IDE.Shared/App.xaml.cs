// Avishai Dernis 2024

using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.StartScreen;
using Zarem.IDE.Helpers;
using Zarem.IDE.Messages;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Files;
using Zarem.IDE.Services.Settings;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.IDE.Windows;

namespace Zarem.IDE;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private readonly IMessenger _messenger;
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        // Setup services
        Services = ConfigureServices();
        Ioc.Default.ConfigureServices(Services);

        // Track necessary services
        _messenger = Service.Get<IMessenger>();
        _settingsService = Service.Get<ISettingsService>();

        // Apply language override from settings
        var localizationService = Service.Get<ILocalizationService>();
        localizationService.LanguageOverride = _settingsService.Local.GetValue<string>(SettingsKeys.LanguageOverride);

        // Subscribe to messages
        _messenger.Register<App, SettingChangedMessage<Theme>>(this, (r, m) => r.ApplyRequestedTheme(m.NewValue));

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

        // Apply theme settings
        LoadRequestedTheme();

        // Initiaize dispatcher on UI thread.
        Services.GetRequiredService<IDispatcherService>().Init();

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

    private void LoadRequestedTheme()
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
