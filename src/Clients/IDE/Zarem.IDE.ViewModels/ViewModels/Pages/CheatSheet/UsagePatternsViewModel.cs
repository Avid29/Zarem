// Avishai Dernis 2026

using System.Collections.ObjectModel;
using Zarem.CheatSheet;
using Zarem.IDE.Bindables.CheatSheet;
using Zarem.IDE.Services;
using Zarem.Mips.Assembler.Models.Tables;

namespace Zarem.IDE.ViewModels.Pages.CheatSheet;

/// <summary>
/// A view model for the usage patterns cheatsheet page.
/// </summary>
public class UsagePatternsViewModel : CheatSheetSubPageViewModel
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsagePatternsViewModel"/> class.
    /// </summary>
    public UsagePatternsViewModel(CheatSheetPage cheatSheet, ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        CheatSheet = cheatSheet;
        Collections = [];

        // TODO: Load the instruction metadata from a service.
        var table = new MipsInstructionTable(new());
        var instructions = table.GetInstructions(false);

        if (cheatSheet.InstructionCollections is null)
            return;

        foreach (var collection in cheatSheet.InstructionCollections)
        {
            Collections.Add(new BindableInstructionCollection(collection, instructions));
        }
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/CheatSheet/InstructionUsagePatternsTitle"];

    /// <summary>
    /// Gets the <see cref="CheatSheetPage"/>.
    /// </summary>
    public CheatSheetPage CheatSheet { get; }

    /// <summary>
    /// Gets a collection of the <see cref="BindableInstructionCollection"/>.
    /// </summary>
    public ObservableCollection<BindableInstructionCollection> Collections { get; }
}
