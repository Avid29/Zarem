// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using System.IO;
using Zarem.ViewModels.Pages;


namespace Zarem.WinUI.Views.Pages.Editor;

public sealed partial class HexEditorPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HexEditorPage"/> class.
    /// </summary>
    public HexEditorPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the <see cref="FilePageViewModel"/>.
    /// </summary>
    public FilePageViewModel? ViewModel
    {
        get;
        set
        {
            field = value;

            var path = value?.File?.Path;
            if (path is null)
                return;

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Reader = new BinaryReader(fileStream);
        }
    }

    private BinaryReader? Reader { get; set; }
}
