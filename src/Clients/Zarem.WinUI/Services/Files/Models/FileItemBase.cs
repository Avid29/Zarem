// Avishai Dernis 2026

using Zarem.Services.Files.Models;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Zarem.WinUI.Services.Files.Models;

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
