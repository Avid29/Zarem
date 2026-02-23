// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zarem.Bindables.Files.Interfaces;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Files.Models;

namespace Zarem.Bindables.Files.Abstract;

/// <summary>
/// An <see cref="BindableFileItem{T}"/> that tracks child items.
/// </summary>
public abstract partial class BindableFileTrackingFileItem<T> : BindableFileItem<T>
    where T : IFileItem
{
    private readonly Dictionary<IBindableFileItem, BindableFile> _virtualParents = [];
    private FileSystemWatcher? _watcher;
    private bool _childNotLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindableFileTrackingFileItem{T}"/> class.
    /// </summary>
    public BindableFileTrackingFileItem(FileService fileService) : base(fileService)
    {
        _childNotLoaded = true;
    }

    /// <summary>
    /// Gets a value indicating whether or not the children have been loaded.
    /// </summary>
    public override bool ChildrenNotLoaded => _childNotLoaded;

    /// <summary>
    /// Gets the folder tracked by the bindable item.
    /// </summary>
    protected abstract IFolder? TrackingFolder { get; set; }

    /// <summary>
    /// Loads the node's children.
    /// </summary>
    public override async Task LoadChildrenAsync(bool recursive = false)
    {
        if (TrackingFolder is null)
            return;

        var items = await TrackingFolder.GetItemsAsync();
        var children = items.Where(x => x.Path != Path).Select(x =>
        {
            return x switch
            {
                IFile file => FileService.TrackFile(file),
                IFolder folder => FileService.TrackFolder(folder),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<IBindableFileItem>(),
            };
        });

        // Is there really no better way
        _childNotLoaded = false;
        OnPropertyChanged(nameof(ChildrenNotLoaded));

        Children.Clear();
        SetupWatcher();

        foreach (var item in children.OrderBy(x => x.Name.EndsWith(".obj")))
        {
            TrackChild(item);

            // Recursively load children if recursing
            if (recursive && item is BindableFolder folder)
                await folder.LoadChildrenAsync(recursive);
        }
    }

    [RelayCommand]
    private async Task CreateChildFileAsync()
    {
        if (TrackingFolder is null)
            return;

        await FileService.FileSystemService.CreateFileByPopupAsync(TrackingFolder.Path);
    }

    internal void TrackChild(IBindableFileItem item)
    {
        var nameAsAsm = $"{System.IO.Path.GetFileNameWithoutExtension(item.Name)}.asm";
        var parentAsm = Children.OfType<BindableFile>().FirstOrDefault(x => x.Name == nameAsAsm);
        if (parentAsm is not null)
        {
            parentAsm.TrackAsChild(item);
            _virtualParents.Add(item, parentAsm);
            return;
        }

        Children.Add(item);
    }

    internal void UntrackChild(IBindableFileItem item)
    {
        if (item is BindableFile file && _virtualParents.ContainsKey(file))
        {
            _virtualParents[file].UntrackChild(item);
            _virtualParents.Remove(file);
        }
        else
        {
            Children.Remove(item);
        }
    }

    private void SetupWatcher()
    {
        if (TrackingFolder is null)
            return;

        _watcher = new(TrackingFolder.Path);
        _watcher.Created += OnChildFileItemCreated;
        _watcher.Deleted += OnChildFileItemDeleted;
        _watcher.Renamed += OnChildFileItemRenamed;
        // TODO: Handle other events

        _watcher.EnableRaisingEvents = true;
    }

    private async void OnChildFileItemCreated(object sender, FileSystemEventArgs e)
    {
        // Retrieve/track the item
        var child = await FileService.GetFileItemAsync(e.FullPath);
        if (child is null)
            return;

        // Track as child
        Service.Get<IDispatcherService>().RunOnUIThread(() =>
        {
            TrackChild(child);
        });
    }

    private async void OnChildFileItemDeleted(object sender, FileSystemEventArgs e)
    {
        // Retrieve the item
        var child = await FileService.GetFileItemAsync(e.FullPath);
        if (child is null)
            return;

        // Untrack the item
        Service.Get<IDispatcherService>().RunOnUIThread(() =>
        {
            FileService.UntrackFileItem(child);
            UntrackChild(child);
        });
    }

    private async void OnChildFileItemRenamed(object sender, RenamedEventArgs e)
    {
        Service.Get<IDispatcherService>().RunOnUIThread(async () =>
        {
            await FileService.RenameTrackedItemAsync(e.OldFullPath, e.FullPath);
        });
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (_watcher is null)
            return;

        _watcher.Created -= OnChildFileItemCreated;
        _watcher.Deleted -= OnChildFileItemDeleted;
        _watcher.Renamed -= OnChildFileItemRenamed;
    }
}
