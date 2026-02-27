// Avishai Dernis 2026

using System;
using Zarem.Models.Interface;

namespace Zarem.Models.Abstract;

/// <summary>
/// An <see cref="IModule"/> that handles file name calculation and display name localization.
/// </summary>
public abstract class ModuleBase : IModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleBase"/> class.
    /// </summary>
    public ModuleBase(string? identity = null)
    {
        Identity = identity ?? $"{Guid.NewGuid()}";
    }

    /// <inheritdoc/>
    public string Identity { get; protected set; }
}
