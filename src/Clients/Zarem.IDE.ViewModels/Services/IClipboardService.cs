// Avishai Dernis 2025

using System.Collections.Generic;
using System.Threading.Tasks;
using Zarem.IDE.Services.Files.Models;

namespace Zarem.IDE.Services;

/// <summary>
/// An interface for a service to manage the clipboard.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Copies a string to the clipboard.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    /// <param name="flush">Whether or not the data should persist outside the app.</param>
    void CopyText(string text, bool flush = true);

    /// <summary>
    /// Copies a collection of file items to the clipboard.
    /// </summary>
    /// <param name="fileItems">The path of the folder to copy.</param>
    /// <param name="flush">Whether or not the data should persist outside the app.</param>
    Task CutFileItemsAsync(IEnumerable<IFileItem> fileItems, bool flush = true);

    /// <summary>
    /// Copies a collection of file items to the clipboard as a cut operation.
    /// </summary>
    /// <param name="fileItems">The path of the file to cut.</param>
    /// <param name="flush">Whether or not the data should persist outside the app.</param>
    Task CopyFileItemsAsync(IEnumerable<IFileItem> fileItems, bool flush = true);
}
