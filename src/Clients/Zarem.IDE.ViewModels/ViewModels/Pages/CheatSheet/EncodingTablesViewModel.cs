// Avishai Dernis 2026

using System.Linq;
using Zarem.IDE.Services;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.IDE.ViewModels.Pages.CheatSheet;

/// <summary>
/// A view model for the encoding pattern page.
/// </summary>
public class EncodingTablesViewModel : CheatSheetSubPageViewModel
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncodingPatternsViewModel"/> class.
    /// </summary>
    public EncodingTablesViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        GPRegisters = [.. Enumerable.Range(0, 32).Select(x => (MipsGpRegister)x)];

    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/CheatSheet/EncodingTablesTitle"];

    /// <summary>
    /// Gets the list of general purpose registers.
    /// </summary>
    public MipsGpRegister[] GPRegisters { get; }
}
