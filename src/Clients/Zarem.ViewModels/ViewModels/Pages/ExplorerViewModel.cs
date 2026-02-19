// Adam Dernis 2024

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.Bindables.Files;
using Zarem.Messages;
using Zarem.Messages.Files;
using Zarem.Models;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Files.Models;
using Zarem.Services.Settings;
using Zarem.ViewModels.Pages.Abstract;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Zarem.Bindables.Files.Interfaces;
using Zarem.Messages.Project;

namespace Zarem.ViewModels.Pages;

/// <summary>
/// A view model for the explorer.
/// </summary>
public partial class ExplorerViewModel : PageViewModel
{
    private const string RecentProjectsCacheKey = "RecentProjects";

    private readonly IMessenger _messenger;
    private readonly ICacheService _cacheService;
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExplorerViewModel"/> class.
    /// </summary>
    public ExplorerViewModel(IMessenger messenger, ICacheService cacheService, IFileService fileService)
    {
        _fileService = fileService;
        _cacheService = cacheService;
        _messenger = messenger;

        RecentProjects = [];

        _ = LoadRecentCacheAsync();

        IsActive = true;
    }

    /// <inheritdoc/>
    public override string Title => "Explorer"; // TODO: Localization

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    public BindableFolder? RootFolder
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (value is not null)
                    Project = null;

                OnPropertyChanged(nameof(RootNode));
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the currently selected project.
    /// </summary>
    public BindableProjectFile? Project
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (value is not null)
                    RootFolder = null;

                OnPropertyChanged(nameof(RootNode));
            }
        }
    }

    /// <summary>
    /// Gets the root node. The TreeView requires this to be an array, but we only have one.
    /// </summary>
    public IEnumerable<IBindableFileItem>? RootNode
    {
        get
        {
            if (Project is not null)
                return [Project];

            if (RootFolder is not null)
                return [RootFolder];

            return null;
        }
    }

    /// <summary>
    /// Gets a list of the recently opened projects and folders.
    /// </summary>
    public ObservableCollection<string> RecentProjects { get; private set; }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _messenger.Register<ExplorerViewModel, FolderOpenedMessage>(this, async (r, m) =>
        {
            var folder = m.Folder;
            if (folder is null)
            {
                r.Project = null;
                r.RootFolder = null;
                return;
            }

            r.RootFolder = await _fileService.GetFolderAsync(folder.Path);
        });

        _messenger.Register<ExplorerViewModel, ProjectOpenedMessage>(this, async (r, m) =>
        {
            var project = m.Project;
            if (project is null)
            {
                r.Project = null;
                r.RootFolder = null;
                return;
            }

            var path = project.Config.ConfigPath;
            Guard.IsNotNull(path);
           
            r.Project = await _fileService.GetProjectFileAsync(path);
        });

        _messenger.Register<ExplorerViewModel, ProjectClosedMessage>(this, (r, m) =>
        {
            r.Project = null;
            r.RootFolder = null;

            _fileService.ClearTracking();
        });

        _messenger.Register<ExplorerViewModel, CacheChangedMessage<RecentFileItemsCache>>(this, async (r, m) => await r.LoadRecentCacheAsync());
    }

    private async Task LoadRecentCacheAsync()
    {
        // Get current cache
        var recent = await _cacheService.RetrieveCacheAsync<RecentFileItemsCache>(RecentProjectsCacheKey);
        if (recent is null)
            return;

        RecentProjects.Clear();
        foreach(var item in recent.Paths)
            RecentProjects.Add(item);
    }

    [RelayCommand]
    private void CreateNewProject()
    {
        Service.Get<MainViewModel>().GoToPageByType<CreateProjectViewModel>();
    }
}
