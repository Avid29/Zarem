// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Zarem.Elf;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.Interfaces;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.IDE.Views.Pages.Editor;

public sealed partial class ModuleEditorPage : UserControl, IFileEditorHandler
{
    public static readonly DependencyProperty ModuleProperty =
        DependencyProperty.Register(nameof(Module), typeof(Module), typeof(ModuleEditorPage), new PropertyMetadata(null));

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

    public Module? Module
    {
        get => (Module?)GetValue(ModuleProperty);
        set => SetValue(ModuleProperty, value);
    }

    public static string FormatOffset(long offset) => $"0x{offset:X8}";

    /// <inheritdoc/>
    public async Task<bool> SaveAsync()
    {
        // TODO: Save module file
        // For now it's view-only

        return false;
    }

    private async Task LoadContentAsync()
    {
        var file = ViewModel?.File;
        if (file is null)
            return;

        // TODO: Dynamically identify the module type
        var stream = await file.FileItem.OpenStreamForReadAsync();
        Module = ElfModule.Open(file.Name, stream)?.Abstract(new());
    }
}
