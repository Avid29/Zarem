// Avishai Dernis 2026

using Zarem.Assembler.Logging;
using Zarem.Linker.Config;
using Zarem.Models;

namespace Zarem.Components.Interfaces;

/// <summary>
/// An interface for a component of a <see cref="Project"/> that links assemblies.
/// </summary>
public interface ILinkerComponent : IProjectComponent
{
    /// <summary>
    /// Gets the emulator config.
    /// </summary>
    LinkerConfig Config { get; }

    /// <summary>
    /// Links an array of modules together
    /// </summary>
    /// <param name="logger">The logger to track any issues that arise.</param>
    /// <param name="modules">The modules to link together.</param>
    /// <returns></returns>
    public Module Link(Logger? logger, params Module[] modules);
}
