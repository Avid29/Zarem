// Avishai Dernis 2026

using System.Collections.ObjectModel;
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

        // TODO: Remove hard coded groups
        PrimaryEncodingPatterns = new(cheatSheet.EncodingPatterns[0]);
        CoProcessor1Patterns = new(cheatSheet.EncodingPatterns[1]);
        CoProcessor0Patterns = new(cheatSheet.EncodingPatterns[2]);
        UniquePatterns = new(cheatSheet.EncodingPatterns[3]);
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/CheatSheet/InstructionEncodingPatternsTitle"];

    /// <summary>
    /// Gets an <see cref="ObservableCollection{EncodingPattern}"/> of the primary encoding patterns.
    /// </summary>
    public ObservableCollection<EncodingPattern> PrimaryEncodingPatterns { get; }

    /// <summary>
    /// Gets an <see cref="ObservableCollection{EncodingPattern}"/> of the coprocessor1 encoding patterns.
    /// </summary>
    public ObservableCollection<EncodingPattern> CoProcessor1Patterns { get; }

    /// <summary>
    /// Gets an <see cref="ObservableCollection{EncodingPattern}"/> of the coprocessor0 encoding patterns.
    /// </summary>
    public ObservableCollection<EncodingPattern> CoProcessor0Patterns { get; }

    /// <summary>
    /// Gets an <see cref="ObservableCollection{EncodingPattern}"/> of unique encoding patterns.
    /// </summary>
    public ObservableCollection<EncodingPattern> UniquePatterns { get; }
}
