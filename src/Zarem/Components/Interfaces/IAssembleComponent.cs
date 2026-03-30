// Avishai Dernis 2026

using System.Threading.Tasks;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Models;
using Zarem.Models.Files;

namespace Zarem.Components.Interfaces;

/// <summary>
/// An interface for a component of a <see cref="Project"/> class for assembling assembly code.
/// </summary>
public interface IAssembleComponent : IProjectComponent
{
    /// <summary>
    /// Gets the assembler config settings.
    /// </summary>
    AssemblerConfig Config { get; }

    /// <summary>
    /// Gets the assembler handler.
    /// </summary>
    IAssemblerHandler Handler { get; }

    /// <summary>
    /// Assembles a file.
    /// </summary>
    /// <param name="file">The source file to assemble.</param>
    /// <param name="rebuild">Whether or not to rebuild if clean.</param>
    /// <param name="logger">The logger to log events.</param>
    /// <returns>The assembler result.</returns>
    Task<AssemblerResult?> AssembleFileAsync(SourceFile file, bool rebuild = true, Logger? logger = null);
}
