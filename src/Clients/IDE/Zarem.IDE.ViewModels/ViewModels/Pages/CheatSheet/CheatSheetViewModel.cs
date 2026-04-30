// Avishai Dernis 2025

using System.Collections.ObjectModel;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages.Abstract;

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

        SubPages = [
                new UsagePatternsViewModel(localizationService),
                new EncodingPatternsViewModel(localizationService),
                new EncodingTablesViewModel(localizationService)
            ];
    }
    
    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/MIPSCheatSheet"];

    /// <summary>
    /// Gets the collection of settings sub-pages.
    /// </summary>
    public ObservableCollection<CheatSheetSubPageViewModel> SubPages { get; }
}
