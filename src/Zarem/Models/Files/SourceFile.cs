// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System.IO;
using Zarem.Models.Breakpoints;

namespace Zarem.Models.Files;

/// <summary>
/// A model for a source file with a reference to a paired object file.
/// </summary>
public class SourceFile : FileBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceFile"/> class.
    /// </summary>
    public SourceFile(IProject project, string fullPath) : base(project, fullPath)
    {
        var directory = Path.GetDirectoryName(FullPath);
        Guard.IsNotNull(directory);

        var saveFileName = Name + ".obj";
        var saveFilePath = Path.Combine(directory, saveFileName);

        ObjectFile = new ObjectFile(this, saveFilePath);
        Breakpoints = new(this);
    }

    /// <summary>
    /// Gets the breakpoints defined in the source file.
    /// </summary>
    public BreakpointCollection Breakpoints { get; }

    /// <summary>
    /// Gets the associates object file.
    /// </summary>
    public ObjectFile ObjectFile { get; }

    /// <summary>
    /// Gets whether or not the source file has unassembled changes.
    /// </summary>
    public bool IsDirty
    {
        get
        {
            // If the binary doesn't exist, the source file is still considered dirty
            if (!ObjectFile.Exists)
                return true;

            var sourceWriteTime = File.GetLastWriteTime(FullPath);
            var objectWriteTime = File.GetLastWriteTime(ObjectFile.FullPath);

            return sourceWriteTime > objectWriteTime;
        }
    }
}
