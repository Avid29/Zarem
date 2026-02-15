// Adam Dernis 2024

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.Bindables.Files;
using Zarem.Messages.Navigation;
using Zarem.Models.Files;
using Zarem.Services;
using Zarem.Services.Windowing;
using Zarem.ViewModels.Pages;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Zarem.ViewModels;

/// <summary>
/// The view model for the root window.
/// </summary>
public partial class WindowViewModel : ObservableRecipient
{
    private readonly IMessenger _messenger;
    private readonly IConsoleService _consoleService;
    private readonly IProjectService _projectService;
    private readonly IWindowingService _windowingService;
    private readonly IBuildService _buildService;
    private readonly IDebugService _debugService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowViewModel"/> class.
    /// </summary>
    public WindowViewModel(
        IMessenger messenger,
        IConsoleService consoleService,
        IProjectService projectService,
        IWindowingService windowingService,
        IBuildService buildService,
        IDebugService debugService,
        MainViewModel mainViewModel,
        PanelViewModel panelViewModel)
    {
        _consoleService = consoleService;
        _messenger = messenger;
        _projectService = projectService;
        _windowingService = windowingService;
        _buildService = buildService;
        _debugService = debugService;

        MainViewModel = mainViewModel;
        PanelViewModel = panelViewModel;

        IsActive = true;

        // Focus the panel when the window is created.
        _messenger.Send(new PanelFocusChangedMessage(PanelViewModel));
    }

    /// <summary>
    /// Gets the <see cref="MainViewModel"/> for the app.
    /// </summary>
    public MainViewModel MainViewModel { get; }

    /// <summary>
    /// Gets the <see cref="ViewModels.PanelViewModel"/> for the panel in the window.
    /// </summary>
    public PanelViewModel PanelViewModel { get; }

    private BindableFile? CurrentFile
    {
        get
        {
            // Get the current page, and ensure it's a file page
            if (MainViewModel.FocusedPanel?.CurrentPage is not FilePageViewModel page)
                return null;

            return page.File;
        }
    }

    private IEnumerable<BindableFile> OpenFiles
    {
        get
        {
            var panel = MainViewModel.FocusedPanel;
            if (panel is null)
                return [];

            return panel.OpenPages.OfType<FilePageViewModel>()
                .Where(x => x.File is not null).Select(x => x.File!);
        }
    }
}
