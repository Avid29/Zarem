// Avishai Dernis 2025

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Zarem.CheatSheet;
using Zarem.Descriptors;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages.Abstract;
using Zarem.Mips.CheatSheet;
using Zarem.Registry;

namespace Zarem.IDE.ViewModels.Pages.CheatSheet;

/// <summary>
/// A view model for the cheatsheet.
/// </summary>
public class CheatSheetViewModel : PageViewModel
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheatSheetViewModel"/> class.
    /// </summary>
    public CheatSheetViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        AvailableArchitectures = ZaremRegistry.Architectures.GetDescriptors().Where(x => x.CheatSheetAssembly is not null);
        SubPages = [];

        Architecture = AvailableArchitectures.FirstOrDefault();
    }
    
    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/CheatSheet"];

    /// <summary>
    /// Gets the selected architecture.
    /// </summary>
    public IArchitectureDescriptor? Architecture
    {
        get;
        set
        {
            if(SetProperty(ref field, value))
            {
                LoadArchitectureCheatSheet(value);
            }
        }
    }

    /// <summary>
    /// Gets the list of available architectures with cheat sheets.
    /// </summary>
    public IEnumerable<IArchitectureDescriptor> AvailableArchitectures { get; }

    /// <summary>
    /// Gets the collection of settings sub-pages.
    /// </summary>
    public ObservableCollection<CheatSheetSubPageViewModel> SubPages { get; }

    private void LoadArchitectureCheatSheet(IArchitectureDescriptor? arch)
    {
        if (arch?.CheatSheetAssembly is null)
            return;

        var page = CheatSheetPage.LoadCheatSheet(arch.CheatSheetAssembly);

        SubPages.Clear();
        SubPages.Add(new UsagePatternsViewModel(page, _localizationService));
        SubPages.Add(new EncodingPatternsViewModel(page, _localizationService));
        SubPages.Add(new EncodingTablesViewModel(_localizationService));
    }
}
