// Adam Dernis 2024

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.Bindables.Files;
using Zarem.Messages;
using Zarem.Messages.Files;
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
using Zarem.Models.Cache;

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

    /// <summary>
    /// Removes an item from the recent projects list.
    /// </summary>
    [RelayCommand]
    public async Task RemoveRecentProjectItem(string item)
    {
        // Remove from collection
        RecentProjects.Remove(item);

        // Get current cache
        var recent = await _cacheService.RetrieveCacheAsync<RecentFileItemsCache>(RecentProjectsCacheKey);
        if (recent is null)
            return;

        // Attempt to find the node
        var node = recent.Paths.Find(item);
        if (node is null)
            return;

        // Remove the node and update the cache
        recent.Paths.Remove(node);
        await _cacheService.CacheAsync(RecentProjectsCacheKey, recent);
    }

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

    [RelayCommand]
    private async Task CreateNewFileAsync()
    {
        if (Project is not null)
        {
            await Project.CreateChildFileAsync();
        }
        else if (RootFolder is not null)
        {
            await RootFolder.CreateChildFileAsync();
        }
    }

    [RelayCommand]
    private async Task CreateNewFolderAsync()
    {
        if (Project is not null)
        {
            await Project.CreateChildFolderAsync();
        }
        else if (RootFolder is not null)
        {
            await RootFolder.CreateChildFolderAsync();
        }
    }
}
