// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.Interfaces;

namespace Zarem.IDE.Views.Pages.Editor;

public sealed partial class ModuleEditorPage : UserControl, IFileEditorHandler
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
            field?.EditorHandler = this;
            _ = LoadContentAsync();
        }
    }

    /// <inheritdoc/>
    public bool IsDirty => false;

    /// <inheritdoc/>
    public async Task<bool> SaveAsync()
    {
        // TODO: Save module file
        return false;
    }

    private async Task LoadContentAsync()
    {
        // TODO: Parse and load module file
    }
}
