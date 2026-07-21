// Avishai Dernis 2025

using Zarem.IDE.Bindables.Files.Interfaces;

namespace Zarem.IDE.Messages.Navigation;

/// <summary>
/// A message sent requesting to open a file.
/// </summary>
public class FileOpenRequestMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileOpenRequestMessage"/> class.
    /// </summary>
    public FileOpenRequestMessage(IBindableFile file)
    {
        File = file;
    }

    /// <summary>
    /// Gets the file to open.
    /// </summary>
    public IBindableFile File { get; }
}
