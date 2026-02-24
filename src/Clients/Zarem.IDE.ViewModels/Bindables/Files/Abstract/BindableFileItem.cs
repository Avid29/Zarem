// Avishai Dernis 2024

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Zarem.Bindables.Files.Interfaces;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Files.Models;

namespace Zarem.Bindables.Files;

/// <summary>
/// A <see cref="IFileItem"/> in the explorer.
/// </summary>
public abstract partial class BindableFileItem : ObservableObject, IBindableFileItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindableFileItem"/> class.
    /// </summary>
    protected BindableFileItem(FileService fileService)
    {
        FileService = fileService;
    }

    /// <summary>
    /// Gets the FileService that owns the file.
    /// </summary>
    protected FileService FileService { get; }

    /// <inheritdoc/>
    public abstract string Name { get; set;  }

    /// <inheritdoc/>
    public abstract string Path { get; }

    /// <inheritdoc/>
    public abstract bool IsFolder { get; }

    /// <summary>
    /// Gets a value indicating whether or not the children have been loaded.
    /// </summary>
    public virtual bool ChildrenNotLoaded => false;

    /// <inheritdoc/>
    public BindableFileItemCollection Children { get; } = [];

    /// <summary>
    /// Loads the node's children.
    /// </summary>
    public virtual async Task LoadChildrenAsync(bool recursive = false)
    {
    }

    /// <summary>
    /// Copies the file name to the clipboard.
    /// </summary>
    /// 
    [RelayCommand]
    public void CopyFileName() => Service.Get<IClipboardService>().CopyText(Name);

    /// <summary>
    /// Copies the file's path to the clipboard.
    /// </summary>
    [RelayCommand]
    public void CopyFilePath() => Service.Get<IClipboardService>().CopyText(Path);

    /// <summary>
    /// Copies the file to the clipboard.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public abstract Task CopyFileAsync();

    /// <summary>
    /// Deletes the file item.
    /// </summary>
    [RelayCommand]
    public abstract Task DeleteAsync();

    /// <inheritdoc/>
    public abstract void Dispose();
}
