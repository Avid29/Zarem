// Avishai Dernis 2025

using System.Collections.Generic;
using System.Threading.Tasks;
using Zarem.Models;
using Zarem.Models.Files;

namespace Zarem.IDE.Services;

/// <summary>
/// An interface for a service to manage the build status.
/// </summary>
public interface IBuildService
{
    /// <summary>
    /// Builds the project.
    /// </summary>
     Task<BuildResult?> BuildProjectAsync(bool rebuild = false);

    /// <summary>
    /// Assembles a set of files.
    /// </summary>
    /// <param name="files">The files to assemble.</param>
     Task<BuildResult?> AssembleFilesAsync(IEnumerable<SourceFile> files);

    /// <summary>
    /// Cleans a project.
    /// </summary>
    public void CleanProject();

    /// <summary>
    /// Cleans a set of files.
    /// </summary>
    /// <param name="files">The files to clean.</param>
    public void CleanFiles(IEnumerable<SourceFile> files);
}
