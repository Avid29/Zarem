// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.Bindables.Files.Abstract;
using Zarem.IDE.Bindables.Files.Abstract;
using Zarem.IDE.Bindables.Files.Interfaces;
using Zarem.IDE.Messages.Navigation;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Files;
using Zarem.IDE.Services.Files.Models;
using Zarem.Models.Files;

namespace Zarem.Bindables.Files;

/// <summary>
/// An <see cref="BindableFileItem{T}"/> for the project
/// </summary>
public partial class BindableProjectFile : BindableFileTrackingFileItem<IFile>, IBindableFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindableProjectFile"/> class.
    /// </summary>
    public BindableProjectFile(FileService fileService, IFile file, IFolder? parent) : base(fileService)
    {
        FileItem = file;
        TrackingFolder = parent;
    }

    /// <inheritdoc/>
    public SourceFile? SourceFile { get; init; }

    /// <inheritdoc/>
    protected override IFolder? TrackingFolder { get; set; }

    /// <inheritdoc/>
    public override IFile FileItem { get; set; }

    /// <inheritdoc/>
    public override bool IsFolder => false;

    /// <inheritdoc/>
    [RelayCommand]
    public void Open() => Service.Get<IMessenger>().Send(new FileOpenRequestMessage(this));
}
