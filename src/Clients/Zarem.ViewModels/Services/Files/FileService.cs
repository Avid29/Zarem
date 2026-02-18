// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Zarem.Bindables.Files;
using Zarem.Bindables.Files.Interfaces;
using Zarem.Services.Files.Models;

namespace Zarem.Services.Files;

/// <summary>
/// A wrapper for the <see cref="IFileSystemService"/> which also tracks open files.
/// </summary>
public class FileService : IFileService
{
    // TODO: Untracking out of use files
    private readonly IProjectService _projectService;
    private readonly Dictionary<string, IBindableFileItem> _openItems;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    public FileService(IFileSystemService fileSystemService, IProjectService projectService)
    {
        FileSystemService = fileSystemService;
        _projectService = projectService;
        
        _openItems = [];
    }

    /// <summary>
    /// Gets the <see cref="IFileSystemService"/>.
    /// </summary>
    public IFileSystemService FileSystemService { get; }

    /// <inheritdoc/>
    public async Task<BindableFolder?> GetFolderAsync(string path)
    {
        // Check if the folder is already tracked, 
        // and retrieve it if so.
        if (TryGetItem(path, out BindableFolder? value))
            return value;

        var folder = await FileSystemService.GetFolderAsync(path);
        if (folder is null)
            return null;

        return TrackFolder(folder);
    }

    /// <inheritdoc/>
    public async Task<BindableFile?> GetFileAsync(string path)
    {
        // Check if the file is already tracked, 
        // and retrieve it if so.
        if (TryGetItem(path, out BindableFile? value))
            return value;

        // Get basic file
        var file = await FileSystemService.GetFileAsync(path);
        if (file is null)
            return null;

        // Create and track new bindable
        return TrackFile(file);
    }

    /// <inheritdoc/>
    public async Task<BindableProjectFile?> GetProjectFileAsync(string path)
    {
        // Check if the file is already tracked, 
        // and retrieve it if so.
        if (TryGetItem(path, out BindableProjectFile? value))
            return value;

        // Get basic file
        var file = await FileSystemService.GetFileAsync(path);
        if (file is null)
            return null;

        // Create and track new bindable
        return TrackProjectFile(file);
    }

    /// <inheritdoc/>
    public async Task<IBindableFileItem?> GetFileItemAsync(string path)
    {
        return Path.HasExtension(path) switch
        {
            true => await GetFileAsync(path),
            false => await GetFolderAsync(path),
        };
    }
    
    /// <inheritdoc/>
    public async Task<BindableFolder?> PickFolderAsync()
    {
        var folder = await FileSystemService.PickFolderAsync();
        if (folder is null)
            return null;

        return TrackFolder(folder);
    }

    /// <inheritdoc/>
    public async Task<BindableFile?> PickFileAsync(params string[] types)
    {
        var file = await FileSystemService.PickFileAsync(types);
        if (file is null)
            return null;

        return TrackFile(file);
    }

    internal BindableFolder TrackFolder(IFolder folder)
    {
        // Check if the folder is already tracked, 
        // and retrieve it if so.
        var key = folder.Path;
        if (TryGetItem(key, out BindableFolder? value))
            return value;

        // Create and track new bindable
        var bindable = new BindableFolder(this, folder);
        _openItems.Add(key, bindable);
        return bindable;
    }

    internal BindableFile TrackFile(IFile file)
    {
        var key = file.Path;

        // If the project file was previously tracked as
        // a project file, untrack the item
        if (TryGetItem(key, out BindableProjectFile? old))
            UntrackFileItem(old);

        // Check if the file is already tracked, 
        // and retrieve it if so.
        if (TryGetItem(key, out BindableFile? value))
            return value;

        // Create and track new bindable
        var bindable = new BindableFile(this, file)
        {
            SourceFile = _projectService.GetSourceFile(key),
        };

        _openItems.Add(key, bindable);
        return bindable;
    }

    internal BindableProjectFile TrackProjectFile(IFile file)
    {
        var key = file.Path;

        // If the project file was previously tracked as
        // a regular file, untrack the item
        if (TryGetItem(key, out BindableFile? old))
            UntrackFileItem(old);

        // Check if the file is already tracked, 
        // and retrieve it if so.
        if (TryGetItem(key, out BindableProjectFile? value))
            return value;

        // Create and track new bindable
        var bindable = new BindableProjectFile(this, file);

        _openItems.Add(key, bindable);
        return bindable;
    }

    internal BindableFileItem TrackFileItem(IFileItem item)
    {
        return item switch
        {
            IFolder folder => TrackFolder(folder),
            IFile file => TrackFileItem(file),
            _ => ThrowHelper.ThrowArgumentException<BindableFileItem>(nameof(item)),
        };
    }

    internal void UntrackFileItem(IBindableFileItem item)
    {
        var key = item.Path;
        if (key is null || !_openItems.Remove(key))
            return;
    }

    internal async Task RenameTrackedItemAsync(string oldPath, string newPath)
    {
        var item = await GetFileItemAsync(oldPath);
        if (item is null)
            return;

        // Untrack item as-is
        UntrackFileItem(item);

        switch (item)
        {
            // Get new folder item child
            case BindableFolder folder:
                var childFolder = await FileSystemService.GetFolderAsync(newPath);
                if (childFolder is null)
                    return;

                folder.FileItem = childFolder;
                break;
                
            // Get new file item child
            case BindableFile file:
                var childFile = await FileSystemService.GetFileAsync(newPath);
                if (childFile is null)
                    return;

                file.FileItem = childFile;
                break;
        }

        // Retrack
        _openItems.Add(newPath, item);
    }

    private bool TryGetItem<T>(string path, [NotNullWhen(true)] out T? item)
        where T : class, IBindableFileItem
    {
        item = null;

        if(!_openItems.TryGetValue(path, out var value))
            return false;

        if (value is not T result)
            return false;

        item = result;
        return true;
    }
}
