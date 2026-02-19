// Avishai Dernis 2025

using System.Collections.Generic;
using System.Threading.Tasks;
using Zarem.Assembler.Logging;
using Zarem.Config;
using Zarem.DebugSessions;
using Zarem.Models;
using Zarem.Models.Files;

namespace Zarem;

/// <summary>
/// A loaded mipser project.
/// </summary>
public interface IProject
{
    /// <summary>
    /// Gets the collection of source files in the project.
    /// </summary>
    SourceCollection SourceFiles { get; }

    /// <summary>
    /// Gets the project's configuration.
    /// </summary>
    IProjectConfig Config { get; }

    /// <summary>
    /// Creates a debug session.
    /// </summary>
    DebugSession? StartDebug();

    /// <summary>
    /// Creates a debug session.
    /// </summary>
    /// <param name="file">The file to use as an entry point.</param>
    Task<DebugSession?> StartDebugAsync(ObjectFile file);

    /// <summary>
    /// Builds the project.
    /// </summary>
    Task<BuildResult> BuildProjectAsync(bool rebuild = false, Logger? logger = null);

    /// <summary>
    /// Assembles a list of files.
    /// </summary>
    Task<BuildResult> AssembleFilesAsync(IEnumerable<SourceFile> files, bool rebuild = true, Logger? logger = null);

    /// <summary>
    /// Cleans all files in the project.
    /// </summary>
    void CleanProject() => CleanFiles(SourceFiles);

    /// <summary>
    /// Cleans a list of source files.
    /// </summary>
    /// <param name="files">The files to clean.</param>
    void CleanFiles(IEnumerable<SourceFile> files);

    /// <summary>
    /// Saves the project configuration.
    /// </summary>
    void Save();
}
