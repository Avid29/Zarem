// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Models.Tables;
using Zarem.IDE.Models.CheatSheet;
using Zarem.IDE.Services;

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
    public UsagePatternsViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        // TODO: Load the instruction metadata from a service.
        var table = new MipsInstructionTable(new());
        var instructions = table.GetInstructions(false);

        CommonInstructions = new(LoadInstructionSet("CommonInstructions.json", instructions) ?? []);
        FloatInstructions = new(LoadInstructionSet("FloatInstructions.json", instructions) ?? []);
        CoProc0Instructions = new(LoadInstructionSet("CoProc0Instructions.json", instructions) ?? []);
        Specialized0Instructions = new(LoadInstructionSet("SpecializedInstructions.json", instructions) ?? []);
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/CheatSheet/InstructionUsagePatternsTitle"];

    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of common instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, MipsInstructionMetaBase>? CommonInstructions { get; }

    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of floating-point instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, MipsInstructionMetaBase>? FloatInstructions { get; }

    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of coproc0 instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, MipsInstructionMetaBase>? CoProc0Instructions { get; }

    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of specialized instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, MipsInstructionMetaBase>? Specialized0Instructions { get; }

    private IEnumerable<IGrouping<string, MipsInstructionMetaBase>>? LoadInstructionSet(string filename, MipsInstructionMetaBase[] instructions)
    {
        // Load groupings
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames();
        var resource = resources.First(x => x.EndsWith(filename));

        using Stream? stream = assembly.GetManifestResourceStream(resource);
        if (stream is null)
            return null;

        var collection = JsonSerializer.Deserialize<InstructionCollection>(stream);
        if (collection is null)
            return null;

        // Create the grouped collection
        var simplePairs = collection.Groups.SelectMany(
            group => group.Instructions.Select(instruction => (group.GroupName, instruction)));
        var pairs = simplePairs.Join(instructions,
            pair => pair.instruction,
            instruction => instruction.Identifier,
            (pair, instruction) => (pair.GroupName, instruction));
        var groups = pairs.GroupBy(x => _localizationService[$"/CheatSheet/InstructionGroup/{x.GroupName}"], x => x.instruction);

        return groups;
    }
}
