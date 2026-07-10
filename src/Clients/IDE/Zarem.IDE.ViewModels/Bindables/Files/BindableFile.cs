// Avishai Dernis 2024

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.IDE.Bindables.Files.Abstract;
using Zarem.IDE.Bindables.Files.Interfaces;
using Zarem.IDE.Messages.Navigation;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Files;
using Zarem.IDE.Services.Files.Models;
using Zarem.Models.Files;

namespace Zarem.Bindables.Files;

/// <summary>
/// A file in the content view or explorer.
/// </summary>
public partial class BindableFile : BindableFileItem<IFile>, IBindableFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindableFile"/> class.
    /// </summary>
    internal BindableFile(FileService fileService, IFile file) : base(fileService)
    {
        FileItem = file;
    }

    /// <inheritdoc/>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    [NotifyPropertyChangedFor(nameof(Path))]
    public override partial IFile FileItem { get; set; }

    /// <inheritdoc/>
    public SourceFile? SourceFile { get; init; }

    /// <inheritdoc/>
    public override bool IsFolder => false;

    internal void TrackAsChild(IBindableFileItem child)
    {
        Children.Add(child);
    }

    internal void UntrackChild(IBindableFileItem child)
    {
        Children.Remove(child);
    }

    /// <inheritdoc/>
    [RelayCommand]
    public void Open() => Service.Get<IMessenger>().Send(new FileOpenRequestMessage(this));

    /// <inheritdoc/>
    public override void Dispose()
    {
    }
}
