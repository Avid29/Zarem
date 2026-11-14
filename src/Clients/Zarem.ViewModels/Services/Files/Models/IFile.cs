// Avishai Dernis 2024

using System.IO;
using System.Threading.Tasks;

namespace Zarem.Services.Files.Models;

/// <summary>
/// An interface for a file.
/// </summary>
public interface IFile : IFileItem
{
    /// <summary>
    /// Opens a <see cref="Stream"/> for reading from the file.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> returning the requested <see cref="FileStream"/>.</returns>
    Task<Stream> OpenStreamForReadAsync();

    /// <summary>
    /// Opens a <see cref="Stream"/> for writing to the file.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> returning the requested <see cref="FileStream"/>.</returns>
    Task<Stream> OpenStreamForWriteAsync();
}
