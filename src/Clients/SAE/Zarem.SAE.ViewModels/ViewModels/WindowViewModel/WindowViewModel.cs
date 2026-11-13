// Avishai Dernis 2024

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.Services.Files;
using Zarem.Services.Settings;
using Zarem.Services.Windowing;

namespace Zarem.SAE.ViewModels;

/// <summary>
/// The view model for the root window.
/// </summary>
public partial class WindowViewModel : ObservableRecipient
{
    private readonly IMessenger _messenger;
    private readonly IFileSystemService _fileSystemService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowingService _windowingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowViewModel"/> class.
    /// </summary>
    public WindowViewModel(
        IMessenger messenger,
        IWindowingService windowingService,
        IFileSystemService fileSystemService,
        ISettingsService settingsService,
        MainViewModel mainViewModel)
    {
        _messenger = messenger;
        _fileSystemService = fileSystemService;
        _settingsService = settingsService;
        _windowingService = windowingService;

        MainViewModel = mainViewModel;

        IsActive = true;
    }

    /// <summary>
    /// Gets the <see cref="MainViewModel"/> for the app.
    /// </summary>
    public MainViewModel MainViewModel { get; }
}
