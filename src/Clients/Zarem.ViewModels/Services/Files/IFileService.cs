// Avishai Dernis 2025

using Zarem.Bindables.Files;
using System.Threading.Tasks;
using Zarem.Bindables.Files.Interfaces;

namespace Zarem.Services.Files;

/// <summary>
/// An interface for a service to manager files in use.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Gets a file from a path.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    public Task<BindableFile?> GetFileAsync(string path);

    /// <summary>
    /// Gets a project file from a path.
    /// </summary>
    /// <param name="path">The path of the project file.</param>
    public Task<BindableProjectFile?> GetProjectFileAsync(string path);

    /// <summary>
    /// Gets a folder from a path.
    /// </summary>
    /// <param name="path">The path of the folder.</param>
    public Task<BindableFolder?> GetFolderAsync(string path);

    /// <summary>
    /// Gets a file item from a path.
    /// </summary>
    /// <param name="path">The path of the file item.</param>
    public Task<IBindableFileItem?> GetFileItemAsync(string path);

    /// <summary>
    /// Opens a file picker to select an <see cref="BindableFile"/>.
    /// </summary>
    /// <returns>The selected <see cref="BindableFile"/>.</returns>
    public Task<BindableFile?> PickFileAsync(params string[] types);

    /// <summary>
    /// Opens a file picker to select an <see cref="BindableFolder"/>.
    /// </summary>
    /// <returns>The selected <see cref="BindableFolder"/>.</returns>
    public Task<BindableFolder?> PickFolderAsync();
}
