// Avishai Dernis 2025

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Zarem.IDE.Services.Files.Models;

namespace Zarem.IDE.Services;

/// <summary>
/// An implementation of the <see cref="IClipboardService"/>
/// </summary>
public class ClipboardService : IClipboardService
{
    /// <inheritdoc/>
    public void CopyText(string text, bool flush = true)
    {
        var package = new DataPackage();
        package.SetText(text);
        SetClipboard(package, DataPackageOperation.Copy, flush);
    }

    /// <inheritdoc/>
    public async Task CutFileItemsAsync(IEnumerable<IFileItem> fileItems, bool flush = true)
        => await ClipFileItemsAsync(fileItems, DataPackageOperation.Move, flush);

    /// <inheritdoc/>
    public async Task CopyFileItemsAsync(IEnumerable<IFileItem> fileItems, bool flush = true)
        => await ClipFileItemsAsync(fileItems, DataPackageOperation.Copy, flush);

    public static async Task ClipFileItemsAsync(IEnumerable<IFileItem> fileItems, DataPackageOperation operation, bool flush = true)
    {
        // Convert the file items to storage items using the assumption they're FileItemBases in implementation
        var storageItems = fileItems.OfType<FileItemBase>().Select(x => x.StorageItem);
        
        var package = new DataPackage();
        package.SetStorageItems(storageItems);
        SetClipboard(package, operation, flush);
    }

    private static void SetClipboard(DataPackage data, DataPackageOperation operation, bool flush = true)
    {
        data.RequestedOperation = operation;
        Clipboard.SetContent(data);

        // Flush?
        if (flush)
        {
            Clipboard.Flush();
        }
    }
}
