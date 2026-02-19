// Avishai Dernis 2026

using System.Threading.Tasks;
using Zarem.Config;
using Zarem.Models;
using Zarem.Models.Files;

namespace Zarem.Components.Interfaces;

/// <summary>
/// An interface for a component of a <see cref="Project"/> that exports formatted binaries.
/// </summary>
public interface IFormatComponent : IProjectComponent
{
    /// <summary>
    /// Gets the object format config.
    /// </summary>
    public FormatConfig Config { get; }

    /// <summary>
    /// Attempts to export a module in the target format.
    /// </summary>
    /// <param name="module">The module to export.</param>
    /// <param name="object">The object file to export to.</param>
    /// <returns><see langword="true"/> if successfully exported, <see langword="false"/> otherwise.</returns>
    Task<bool> TryExportAsync(Module module, ObjectFile @object);

    /// <summary>
    /// Attempts to import a module in the target format.
    /// </summary>
    /// <param name="object">The object file to import.</param>
    /// <returns>The file imported, or null if unsuccessful.</returns>
    Task<Module?> ImportAsync(ObjectFile @object);
}
