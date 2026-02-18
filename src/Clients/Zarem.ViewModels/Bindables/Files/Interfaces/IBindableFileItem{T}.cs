// Avishai Dernis 2026

using Zarem.Services.Files.Models;

namespace Zarem.Bindables.Files.Interfaces;

/// <summary>
/// An interface for an <see cref="IFileItem"/> in the explorer.
/// </summary>
public interface IBindableFileItem<T> : IBindableFileItem
    where T : IFileItem
{
    /// <summary>
    /// The wrapped <see cref="IFileItem"/>.
    /// </summary>
    public abstract T FileItem { get; set; }
}
