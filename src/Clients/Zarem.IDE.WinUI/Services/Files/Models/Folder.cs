// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Zarem.Services.Files.Models;

namespace Zarem.WinUI.Services.Files.Models;

/// <summary>
/// An <see cref="IFolder"/> implementation wrapping a <see cref="StorageFolder"/>.
/// </summary>
public class Folder : FileItemBase, IFolder
{
    private readonly StorageFolder _storageFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="Folder"/> class.
    /// </summary>
    /// <param name="storageFolder"></param>
    public Folder(StorageFolder storageFolder)
    {
        _storageFolder = storageFolder;
    }

    /// <inheritdoc/>
    public override IStorageItem StorageItem => _storageFolder;

    /// <inheritdoc/>
    public async Task<IFileItem[]> GetItemsAsync()
    {
        var items = await _storageFolder.GetItemsAsync();
        return items.Select(x =>
        {
            return x switch
            {
                StorageFile file => new File(file),
                StorageFolder folder => new Folder(folder),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<IFileItem>(),
            };
        }).ToArray();
    }

    /// <inheritdoc/>
    public async Task<IFolder[]> GetFoldersAsync()
    {
        var folders = await _storageFolder.GetFoldersAsync();
        return folders.Select(x => (IFolder)new Folder(x)).ToArray();
    }

    /// <inheritdoc/>
    public async Task<IFile[]> GetFilesAsync()
    {
        var files = await _storageFolder.GetFilesAsync();
        return files.Select(x => (IFile)new File(x)).ToArray();
    }
}
