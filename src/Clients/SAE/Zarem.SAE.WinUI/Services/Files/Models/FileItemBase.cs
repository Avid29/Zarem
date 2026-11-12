// Avishai Dernis 2026

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Zarem.Services.Files.Models;

namespace Zarem.SAE.Services.Files.Models;

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
