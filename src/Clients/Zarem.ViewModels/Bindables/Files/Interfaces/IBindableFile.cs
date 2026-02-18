// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Input;
using Zarem.Models.Files;
using Zarem.Services.Files.Models;

namespace Zarem.Bindables.Files.Interfaces;

/// <summary>
/// An <see cref="IBindableFileItem"/> for an <see cref="IFile"/>
/// </summary>
public partial interface IBindableFile : IBindableFileItem<IFile>
{
    /// <summary>
    /// Gets the associate <see cref="SourceFile"/>.
    /// </summary>
    SourceFile? SourceFile { get; init; }

    /// <summary>
    /// Open the file.
    /// </summary>
    [RelayCommand]
    public void Open();
}
