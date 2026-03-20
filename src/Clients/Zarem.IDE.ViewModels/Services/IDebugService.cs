// Avishai Dernis 2026

using System.Threading.Tasks;
using Zarem.Debugger.Models.Enums;
using Zarem.Models.Files;
using Zarem.Models.Tables;

namespace Zarem.IDE.Services;

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

    /// <summary>
    /// Resumes a currently paused debugger.
    /// </summary>
    void Continue();

    /// <summary>
    /// Steps the current debugger.
    /// </summary>
    /// <param name="mode">The type of step to perform.</param>
    public void Step(StepMode mode);

    /// <summary>
    /// Stops the current debug session.
    /// </summary>
    void StopDebugging();

    /// <summary>
    /// Gets or sets the currently executing line.
    /// </summary>
    public SourceRange? ExecutingLocation { get; set; }
}
