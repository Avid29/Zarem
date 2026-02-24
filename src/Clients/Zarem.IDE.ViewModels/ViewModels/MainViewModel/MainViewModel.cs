// Avishai Dernis 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.ViewModels.Pages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Files;

namespace Zarem.IDE.ViewModels;

/// <summary>
/// The main view model for the application.
/// </summary>
public partial class MainViewModel : ObservableRecipient
{
    private readonly IMessenger _messenger;
    private readonly IProjectService _projectService;
    private readonly IBuildService _buildService;
    private readonly IFileService _fileService;
    private readonly IFileSystemService _fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel(
        IMessenger messenger,
        IBuildService buildService,
        IFileService fileService,
        IFileSystemService fileSystemService,
        IProjectService projectService,
        StatusViewModel statusViewModel,
        ExplorerViewModel explorerViewModel)
    {
        _messenger = messenger;
        _projectService = projectService;
        _buildService = buildService;
        _fileService = fileService;
        _fileSystemService = fileSystemService;

        StatusViewModel = statusViewModel;
        ExplorerViewModel = explorerViewModel;
        Panels = [];

        IsActive = true;
    }

    /// <summary>
    /// Gets the currently focused <see cref="PanelViewModel"/>.
    /// </summary>
    public PanelViewModel? FocusedPanel
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets a list of all panels.
    /// </summary>
    public List<PanelViewModel> Panels { get; }

    /// <summary>
    /// Gets the <see cref="StatusViewModel"/>.
    /// </summary>
    public StatusViewModel StatusViewModel { get; }

    /// <summary>
    /// Gets the <see cref="ExplorerViewModel"/>.
    /// </summary>
    public ExplorerViewModel ExplorerViewModel { get; }

    /// <summary>
    /// Saves all open files.
    /// </summary>
    public async Task SaveAllFilesAsync()
    {
        foreach (var panel in Panels)
        {
            await panel.SaveAllFilesAsync();
        }
    }
}
