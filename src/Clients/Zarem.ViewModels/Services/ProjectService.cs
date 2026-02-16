// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using ObjFormats.RASM;
using System.IO;
using System.Threading.Tasks;
using Zarem.Config;
using Zarem.Elf;
using Zarem.Messages.Files;
using Zarem.MIPS;
using Zarem.Models;
using Zarem.Models.Files;
using Zarem.RASM;
using Zarem.Registry;
using Zarem.Serialization;
using Zarem.Services.Files;
using Zarem.Services.Files.Models;
using Zarem.Services.Settings;

namespace Zarem.Services;

/// <summary>
/// An implementation of the <see cref="IProjectService"/> interface.
/// </summary>
public class ProjectService : IProjectService
{
    private const string RecentProjectsCacheKey = "RecentProjects";

    private readonly IMessenger _messenger;
    private readonly ICacheService _cacheService;
    private readonly IFileSystemService _fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectService"/> class.
    /// </summary>
    public ProjectService(IMessenger messenger, ISettingsService settingsService, ICacheService cacheService, IFileSystemService fileSystemService)
    {
        _messenger = messenger;
        _cacheService = cacheService;
        _fileSystemService = fileSystemService;

        // Populate
        ZaremRegistry.RegisterArchitecture(new MIPSArchitectureDescriptor());
        ZaremRegistry.Formats.Register(new ElfModuleDescriptor());
        //ZaremRegistry.Formats.Register(new RasmModuleDescriptor());
    }

    /// <inheritdoc/>
    public IProject? Project { get; private set; }

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
    public async Task OpenFolderAsync(string path, bool cacheState = true)
    {
        // Load the folder
        var folder = await _fileSystemService.GetFolderAsync(path);
        if (folder is null)
            return;

        // Open the folder
        OpenFolder(folder, cacheState);
    }

    /// <inheritdoc/>
    public async Task OpenPathAsyc(string path, bool cacheState = true)
    {
        if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
        {
            await OpenFolderAsync(path);
        }
        else
        {
            await OpenProjectAsync(path);
        }
    }

    /// <inheritdoc/>
    public async Task OpenProjectAsync(IProjectConfig config, bool cacheState = true)
    {
        Project = ProjectFactory.Create(config);
        if (Project?.Config?.RootFolderPath is null)
            return;

        await OpenFolderAsync(Project.Config.RootFolderPath, false);

        if (cacheState)
        {
            await CacheOpenProjectAsync();
        }
    }

    /// <inheritdoc/>
    public async Task OpenProjectAsync(string path, bool cacheState = true)
    {
        // Attempt to load the file
        var file = await _fileSystemService.GetFileAsync(path);
        if (file is null)
            return;

        var project = ProjectFactory.Load(path);

        // Open the project
        await OpenProjectAsync(project.Config, cacheState);
    }

    /// <inheritdoc/>
    public async Task CloseProjectAsync()
    {
        Project = null;
        OpenFolder(null, false);
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
