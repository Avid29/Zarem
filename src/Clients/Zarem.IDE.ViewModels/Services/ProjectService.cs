// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.IO;
using System.Threading.Tasks;
using Zarem.Config;
using Zarem.Elf;
using Zarem.IDE.Messages.Files;
using Zarem.IDE.Messages.Project;
using Zarem.IDE.Models.Cache;
using Zarem.IDE.Models.Enums;
using Zarem.IDE.Services.Files;
using Zarem.IDE.Services.Files.Models;
using Zarem.IDE.Services.Popup;
using Zarem.IDE.Services.Popup.Models;
using Zarem.MIPS;
using Zarem.MIPS.TrapHandlers;
using Zarem.Models.Files;
using Zarem.Registry;
using Zarem.Serialization;

namespace Zarem.IDE.Services;

/// <summary>
/// An implementation of the <see cref="IProjectService"/> interface.
/// </summary>
public class ProjectService : IProjectService
{
    private const string RecentProjectsCacheKey = "RecentProjects";

    private readonly IMessenger _messenger;
    private readonly ICacheService _cacheService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILocalizationService _localizationService;
    private readonly IPopupService _popupService;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectService"/> class.
    /// </summary>
    public ProjectService(
        IMessenger messenger,
        ICacheService cacheService,
        IFileSystemService fileSystemService,
        ILocalizationService localizationService,
        IPopupService popupService,
        IStateService stateService)
    {
        _messenger = messenger;
        _cacheService = cacheService;
        _fileSystemService = fileSystemService;
        _localizationService = localizationService;
        _popupService = popupService;
        _stateService = stateService;

        // Populate
        ZaremRegistry.RegisterArchitecture(new MipsArchitectureDescriptor());
        ZaremRegistry.Formats.Register(new ElfModuleDescriptor());
        //ZaremRegistry.Formats.Register(new RasmModuleDescriptor());
        ZaremRegistry.TrapHandlers.Register(new ZaremTrapHandlerDescriptor());
    }

    /// <inheritdoc/>
    public IProject? Project
    {
        get => field;
        set
        {
            field = value;
            _stateService.SetState(value is not null ? IdeState.Ready : IdeState.NotReady);
        }
    }

    /// <inheritdoc/>
    public IFolder? ProjectRootFolder { get; private set; }

    /// <inheritdoc/>
    public SourceFile? GetSourceFile(string filePath)
    {
        if (Project is null)
            return null;

        return Project.SourceFiles[filePath];
    }

    /// <inheritdoc/>
    public void OpenFolder(IFolder? folder, bool cacheState = true)
    {
        // Change the root folder
        ProjectRootFolder = folder;

        // Send a message notifying the change
        _messenger.Send(new FolderOpenedMessage(folder));

        // Update the state
        if (cacheState && folder is not null)
        {
            _ = CacheOpenProjectAsync(true);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> OpenFolderAsync(string path, bool cacheState = true)
    {
        // Load the folder
        var folder = await _fileSystemService.GetFolderAsync(path);
        if (folder is null)
            return false;

        // Open the folder
        OpenFolder(folder, cacheState);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> OpenPathAsync(string path, bool cacheState = true)
    {
        if(!File.Exists(path))
            return false;

        if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
        {
            return await OpenFolderAsync(path);
        }
        else
        {
            return await OpenProjectAsync(path);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> OpenProjectAsync(IProjectConfig config, bool cacheState = true)
    {
        if (Project is not null)
        {
            // Notify that the project was closed.
            _messenger.Send(new ProjectClosedMessage(Project));
        }

        Project = ProjectFactory.Create(config);
        if (Project?.Config?.RootFolderPath is null)
            return false;

        // Notify that the project was opened.
        _messenger.Send(new ProjectOpenedMessage(Project));

        if (cacheState)
        {
            await CacheOpenProjectAsync();
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> OpenProjectAsync(string path, bool cacheState = true)
    {
        // Attempt to load the file
        var file = await _fileSystemService.GetFileAsync(path);
        if (file is null)
            return false;

        try
        {
            var project = ProjectFactory.Load(path);

            // Open the project
            return await OpenProjectAsync(project.Config, cacheState);
        }
        catch (Exception)
        {
            var popup = new PopupDetails(_localizationService["/Popups/CouldNotOpenProjectTitle"])
            {
                Description = _localizationService["/Popups/CouldNotOpenProjectDescription"],
                CloseButtonText = _localizationService["/Popups/Close"],
            };
            await _popupService.ShowPopupAsync(popup);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task CloseProjectAsync()
    {
        var project = Project;

        // Close the project
        Project = null;

        if (project is not null)
        {
            // Notify that the project was closed.
            _messenger.Send(new ProjectClosedMessage(project));
        }
    }

    private async Task CacheOpenProjectAsync(bool folder = false)
    {
        // Get current cache
        var recent = await _cacheService.RetrieveCacheAsync<RecentFileItemsCache>(RecentProjectsCacheKey);
        if (recent is null)
            recent = new();

        // Append proper path
        var path = folder switch
        {
            false => Project?.Config?.ConfigPath,
            true => ProjectRootFolder?.Path,
        };
        recent.Append(path, 10);

        // Cache updated cache model
        await _cacheService.CacheAsync(RecentProjectsCacheKey, recent);
    }
}
