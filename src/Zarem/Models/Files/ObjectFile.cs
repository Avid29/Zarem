// Avishai Dernis 2025


using System.IO;

namespace Zarem.Models.Files;

/// <summary>
/// A model for a source file with a reference to a paired object file.
/// </summary>
public class ObjectFile : FileBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectFile"/> class.
    /// </summary>
    internal ObjectFile(SourceFile sourceFile, string fullPath) : base(sourceFile.Project, fullPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectFile"/> class.
    /// </summary>
    internal ObjectFile(IProject project, string fullPath) : base(project, fullPath)
    {
    }

    /// <summary>
    /// Gets the associates source file.
    /// </summary>
    public SourceFile? SourceFile { get; internal set; }

    /// <summary>
    /// Gets if the object file currently exists.
    /// </summary>
    public bool Exists => File.Exists(FullPath);
}
