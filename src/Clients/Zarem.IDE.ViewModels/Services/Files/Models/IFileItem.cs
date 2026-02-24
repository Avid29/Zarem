// Avishai Dernis 2024

using System.Threading.Tasks;

namespace Zarem.IDE.Services.Files.Models;

/// <summary>
/// An interface for a files item.
/// </summary>
public interface IFileItem
{
    /// <summary>
    /// Gets the name of the files item.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the path of the files item.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Attempts to rename the file item.
    /// </summary>
    /// <param name="desiredName">The desired new name for the file.</param>
    Task RenameAsync(string desiredName);

    /// <summary>
    /// Delete the file item.
    /// </summary>
    Task DeleteAsync();
}
