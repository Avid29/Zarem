// Avishai Dernis 2025

using Zarem.Services.Files.Models;

namespace Zarem.Messages.Files;

/// <summary>
/// A message sent when a folder is opened.
/// </summary>
public class FolderOpenedMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FolderOpenedMessage"/> class.
    /// </summary>
    public FolderOpenedMessage(IFolder? folder)
    {
        Folder = folder;
    }

    /// <summary>
    /// Gets the opened folder.
    /// </summary>
    public IFolder? Folder { get; }
}
