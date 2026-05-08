// Avishai Dernis 2024

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Zarem.IDE.Bindables.Files.Interfaces;
using Zarem.IDE.Messages.Navigation;
using Zarem.IDE.Messages.Project;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Windowing;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.ViewModels;

/// <summary>
/// The view model for the root window.
/// </summary>
public partial class WindowViewModel : ObservableRecipient
{
    private readonly IMessenger _messenger;
    private readonly IBuildService _buildService;
    private readonly IConsoleService _consoleService;
    private readonly IDebugService _debugService;
    private readonly IProjectService _projectService;
    private readonly IWindowingService _windowingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowViewModel"/> class.
    /// </summary>
    public WindowViewModel(
        IMessenger messenger,
        IBuildService buildService,
        IConsoleService consoleService,
        IDebugService debugService,
        IProjectService projectService,
        IWindowingService windowingService,
        MainViewModel mainViewModel,
        PanelViewModel panelViewModel)
    {
        _messenger = messenger;
        _buildService = buildService;
        _consoleService = consoleService;
        _debugService = debugService;
        _projectService = projectService;
        _windowingService = windowingService;

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

    private IBindableFile? CurrentFile
    {
        get
        {
            // Get the current page, and ensure it's a file page
            if (MainViewModel.FocusedPanel?.CurrentPage is not FilePageViewModel page)
                return null;

            return page.File;
        }
    }

    private IEnumerable<IBindableFile> OpenFiles
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

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _messenger.Register<WindowViewModel, ProjectClosedMessage>(this, async (r, m) =>
        {
            await PanelViewModel.ClosePagesAsync();
        });
    }
}
