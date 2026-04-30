// Avishai Dernis 2024

using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Zarem.Bindables.Files.Abstract;
using Zarem.IDE.Bindables.Files.Interfaces;
using Zarem.IDE.Services.Files;
using Zarem.IDE.Services.Files.Models;

namespace Zarem.Bindables.Files;

/// <summary>
/// A folder in the explorer.
/// </summary>
public partial class BindableFolder : BindableFileTrackingFileItem<IFolder>, IBindableFileItem<IFolder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindableFolder"/> class.
    /// </summary>
    public BindableFolder(FileService fileService, IFolder folder) : base(fileService)
    {
        FileItem = folder;
    }

    /// <inheritdoc/>
    public override IFolder FileItem
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Path));
                TrackingFolder = value;
            }
        }
    }

    /// <inheritdoc/>
    protected override IFolder? TrackingFolder { get; set; }

    /// <inheritdoc/>
    public override bool IsFolder => true;

    /// <summary>
    /// Open the windows file explorer to the folder.
    /// </summary>
    [RelayCommand]
    public void OpenInExplorer() => Process.Start("explorer.exe", $"\"{Path}\"");

    /// <summary>
    /// Open the windows terminal to the folder.
    /// </summary>
    [RelayCommand]
    public void OpenInWindowsTerminal() => Process.Start("wt.exe", $"-d \"{Path}\"");
}
