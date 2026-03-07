// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.Views.Pages.Editor;

public sealed partial class ModuleEditorPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleEditorPage"/> class.
    /// </summary>
    public ModuleEditorPage()
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
        }
    }
}
