// Avishai Dernis 2026

using System.Threading.Tasks;
using Zarem.Models.Files;

namespace Zarem.Services;

/// <summary>
/// An interface for a service that manages debug sessions.
/// </summary>
public interface IDebugService
{
    /// <summary>
    /// Runs the project.
    /// </summary>
    /// <param name="debug">Whether or not to attach a debugger.</param>
    Task RunAsync(bool debug = true);

    /// <summary>
    /// Runs a file.
    /// </summary>
    /// <param name="file">The source file to execute.</param>
    /// <param name="debug">Whether or not to attach a debugger.</param>
    Task RunFileAsync(SourceFile file, bool debug = true);
}
