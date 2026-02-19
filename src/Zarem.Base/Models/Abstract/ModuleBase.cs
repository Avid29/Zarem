// Avishai Dernis 2026

using System.IO;
using Zarem.Localization;
using Zarem.Models.Interface;

namespace Zarem.Models.Abstract;

/// <summary>
/// An <see cref="IModule"/> that handles file name calculation and display name localization.
/// </summary>
public abstract class ModuleBase : IModule
{
    /// <inheritdoc/>
    public string? FilePath { get; protected set; }

    /// <inheritdoc/>
    public string? FileName => Path.GetFileName(FilePath);

    /// <inheritdoc/>
    public string DisplayName => FileName ?? Localizer.Resources["AnonymousModule"] ?? "";
}
