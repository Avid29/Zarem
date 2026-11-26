// Avishai Dernis 2024

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.SAE.Services;
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
    private readonly IEmulatorService _emulatorService;
    private readonly IWindowingService _windowingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowViewModel"/> class.
    /// </summary>
    public WindowViewModel(
        IMessenger messenger,
        IEmulatorService emulatorService,
        IWindowingService windowingService,
        MainViewModel mainViewModel,
        OverlayViewModel overlayViewModel)
    {
        _messenger = messenger;
        _emulatorService = emulatorService;
        _windowingService = windowingService;

        MainViewModel = mainViewModel;
        OverlayViewModel = overlayViewModel;

        IsActive = true;
    }

    /// <summary>
    /// Gets the <see cref="MainViewModel"/> for the app.
    /// </summary>
    public MainViewModel MainViewModel { get; }

    /// <summary>
    /// Gets the <see cref="OverlayViewModel"/> for the window.
    /// </summary>
    public OverlayViewModel OverlayViewModel { get; }
}
