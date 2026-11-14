// Avishai Dernis 2024

using System.Threading.Tasks;

namespace Zarem.Services.Files.Models;

/// <summary>
/// An interface for a folder.
/// </summary>
public interface IFolder : IFileItem
{
    /// <summary>
    /// Gets the files and subfolders in this folder.
    /// </summary>
    Task<IFileItem[]> GetItemsAsync();

    /// <summary>
    /// Gets the subfolders in this folder.
    /// </summary>
    Task<IFolder[]> GetFoldersAsync();

    /// <summary>
    /// Gets the files in this folder.
    /// </summary>
    Task<IFile[]> GetFilesAsync();
}
