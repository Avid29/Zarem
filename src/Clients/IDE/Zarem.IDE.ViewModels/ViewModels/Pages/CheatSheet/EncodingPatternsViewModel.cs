// Avishai Dernis 2026

using Zarem.CheatSheet;
using Zarem.IDE.Services;

namespace Zarem.IDE.ViewModels.Pages.CheatSheet;

/// <summary>
/// A view model for the encoding pattern page.
/// </summary>
public class EncodingPatternsViewModel : CheatSheetSubPageViewModel
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncodingPatternsViewModel"/> class.
    /// </summary>
    public EncodingPatternsViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/CheatSheet/InstructionEncodingPatternsTitle"];
}
