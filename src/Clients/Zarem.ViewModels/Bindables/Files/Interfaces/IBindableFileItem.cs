// Avishai Dernis 2026

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Zarem.Services.Files.Models;

namespace Zarem.Bindables.Files.Interfaces;

/// <summary>
/// An interface for an <see cref="IFileItem"/> in the explorer.
/// </summary>
public interface IBindableFileItem : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the file's path.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets a value indicating whether or not the children have been loaded.
    /// </summary>
    public bool ChildrenNotLoaded { get; }

    /// <summary>
    /// Gets the child items.
    /// </summary>
    ObservableCollection <IBindableFileItem> Children { get; }

    /// <summary>
    /// Loads the node's children.
    /// </summary>
    Task LoadChildrenAsync(bool recursive = false);
}
