// Adam Dernis 2024

using Zarem.Services.Files.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Zarem.WinUI.Services.Files.Models;

/// <summary>
/// An <see cref="IFile"/> implementation wrapping <see cref="StorageFile"/>.
/// </summary>
public sealed class File : FileItemBase, IFile
{
    private readonly StorageFile _storageFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class.
    /// </summary>
    public File(StorageFile storageFile)
    {
        _storageFile = storageFile;
    }

    /// <inheritdoc/>
    public override IStorageItem StorageItem => _storageFile;

    /// <inheritdoc/>
    public Task<Stream> OpenStreamForReadAsync() => _storageFile.OpenStreamForReadAsync();

    /// <inheritdoc/>
    public Task<Stream> OpenStreamForWriteAsync() => _storageFile.OpenStreamForWriteAsync();
}
