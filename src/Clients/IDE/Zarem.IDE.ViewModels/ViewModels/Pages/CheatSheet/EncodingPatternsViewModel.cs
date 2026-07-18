// Avishai Dernis 2026

using System.Collections.ObjectModel;
using System.Linq;
using Zarem.CheatSheet;
using Zarem.CheatSheet.Models;
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
    public EncodingPatternsViewModel(CheatSheetPage cheatSheet, ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        CheatSheet = cheatSheet;
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/CheatSheet/InstructionEncodingPatternsTitle"];

    /// <summary>
    /// Gets the <see cref="CheatSheetPage"/>.
    /// </summary>
    public CheatSheetPage CheatSheet { get; }
}
