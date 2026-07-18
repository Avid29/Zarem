// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Zarem.Elf;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.Interfaces;
using Zarem.Models;
using Zarem.Models.Tables;
using Symbol = Zarem.Models.Tables.Symbol;

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

    public ObservableCollection<Symbol> Symbols { get; } = [];

    public ObservableCollection<RelocationEntry> Relocations { get; } = [];

    public static string FormatOffset(Address address)
    {
        if (address.Section is null)
        {
            return string.Empty;
        }

        return $"0x{address.Offset:X8}";
    }

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
        var module = ElfModule.Open(file.Name, stream)?.Abstract(new());
        LoadModule(module);
    }

    private void LoadModule(Module? module)
    {
        Symbols.Clear();
        Relocations.Clear();

        if (module is null)
            return;

        foreach (var symbol in module.Symbols.Values)
        {
            Symbols.Add(symbol);
        }

        foreach (var section in module.Sections.Values)
        {
            foreach (var reloc in section.Relocations)
            {
                Relocations.Add(reloc);
            }
        }
    }
}
