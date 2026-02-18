// Adam Dernis 2024

using System.Threading.Tasks;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Files.Models;

namespace Zarem.Bindables.Files.Abstract;

/// <summary>
/// A <see cref="IFileItem"/> in the explorer.
/// </summary>
public abstract class BindableFileItem<T> : BindableFileItem
    where T : IFileItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindableFileItem"/> class.
    /// </summary>
    protected BindableFileItem(FileService fileService) : base(fileService)
    {
    }

    /// <summary>
    /// The wrapped <see cref="IFileItem"/>.
    /// </summary>
    public abstract T FileItem { get; set; }

    /// <inheritdoc/>
    public override string Name
    {
        get => FileItem.Name;
        set => FileItem.RenameAsync(value);
    }

    /// <inheritdoc/>
    public override string Path => FileItem.Path;

    /// <summary>
    /// Copies the file to the clipboard.
    /// </summary>
    /// <returns></returns>
    public override async Task CopyFileAsync() => await Service.Get<IClipboardService>().CopyFileItemsAsync([FileItem]);

    /// <inheritdoc/>
    public override async Task DeleteAsync() => await Service.Get<IFileSystemService>().DeleteFileItemAsync(FileItem);
}
