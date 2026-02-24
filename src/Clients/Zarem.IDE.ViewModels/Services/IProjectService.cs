// Avishai Dernis 2025

using Zarem.Config;
using Zarem.Models.Files;
using Zarem.Services.Files.Models;
using System.Threading.Tasks;

namespace Zarem.Services;

/// <summary>
/// An interface for a service that manages project layout.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets the open project.
    /// </summary>
    public IProject? Project { get; }

    /// <summary>
    /// Gets the root folder of the project.
    /// </summary>
    public IFolder? ProjectRootFolder { get; }

    /// <summary>
    /// Gets the source file from a file path.
    /// </summary>
    /// <param name="filePath">The path of the source file.</param>
    /// <returns></returns>
    public SourceFile? GetSourceFile(string filePath);

    /// <summary>
    /// Opens a folder as the new project folder.
    /// </summary>
    public void OpenFolder(IFolder folder, bool cacheState = true);

    /// <summary>
    /// Opens a folder or project by path.
    /// </summary>
    public Task<bool> OpenPathAsync(string path, bool cacheState = true);

    /// <summary>
    /// Opens a folder as the new project folder by path.
    /// </summary>
    public Task<bool> OpenFolderAsync(string path, bool cacheState = true);

    /// <summary>
    /// Opens a project by config.
    /// </summary>
    public Task<bool> OpenProjectAsync(IProjectConfig config, bool cacheState = true);

    /// <summary>
    /// Opens a project by config path.
    /// </summary>
    public Task<bool> OpenProjectAsync(string filePath, bool cacheState = true);

    /// <summary>
    /// Closes the currently open project/folder.
    /// </summary>
    public Task CloseProjectAsync();
}
