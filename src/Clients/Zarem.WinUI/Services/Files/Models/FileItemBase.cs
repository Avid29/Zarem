// Avishai Dernis 2026

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Zarem.Services.Files.Models;

namespace Zarem.Services.Files.Models;

/// <summary>
/// A base class for file items that provides common functionality for file operations.
/// </summary>
public abstract class FileItemBase : IFileItem
{
    /// <inheritdoc/>
    public string Name => StorageItem.Name;

    /// <inheritdoc/>
    public string Path => StorageItem.Path;

    /// <summary>
    /// Gets the underlying <see cref="IStorageItem"/>.
    /// </summary>
    public abstract IStorageItem StorageItem { get; }

    /// <inheritdoc/>
    public async Task DeleteAsync() => await StorageItem.DeleteAsync();

    /// <inheritdoc/>
    public async Task RenameAsync(string desiredName) => await StorageItem.RenameAsync(desiredName);
}
