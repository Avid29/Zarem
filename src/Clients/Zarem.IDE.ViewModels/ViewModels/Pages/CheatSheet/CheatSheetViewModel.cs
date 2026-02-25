// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Zarem.Assembler.Models;
using Zarem.IDE.Models.CheatSheet;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages.Abstract;
using Zarem.Models.Instructions.Enums.Registers;

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

        // TODO: Load the instruction metadata from a service.
        var table = new InstructionTable(new());
        var instructions = table.GetInstructions(false);

        CommonInstructions = new(LoadInstructionSet("CommonInstructions.json", instructions) ?? []);
        FloatInstructions = new(LoadInstructionSet("FloatInstructions.json", instructions) ?? []);
        CoProc0Instructions = new(LoadInstructionSet("CoProc0Instructions.json", instructions) ?? []);
        Specialized0Instructions = new(LoadInstructionSet("SpecializedInstructions.json", instructions) ?? []);

        PrimaryEncodingPatterns = new(LoadEncodingPatterns("PrimaryEncodings.json") ?? []);
        CoProcessor1Patterns = new(LoadEncodingPatterns("CoProcessor1Encodings.json") ?? []);
        CoProcessor0Patterns = new(LoadEncodingPatterns("CoProcessor0Encodings.json") ?? []);
        UniquePatterns = new(LoadEncodingPatterns("UniqueEncodings.json") ?? []);

        GPRegisters = [..Enumerable.Range(0, 32).Select(x => (GPRegister)x)];

        IsActive = true;
    }
    
    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/MIPSCheatSheet"];

    private EncodingPattern[]? LoadEncodingPatterns(string filename)
    {
        // Get resources
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames();
        var resource = resources.First(x => x.EndsWith(filename));
        using Stream? stream = assembly.GetManifestResourceStream(resource);
        if (stream is null)
            return null;

        // Deserialize patterns
        var patterns = JsonSerializer.Deserialize<EncodingPattern[]>(stream);
        if (patterns is null)
            return null;

        // Localize
        foreach (var pattern in patterns)
        {
            if (pattern.Name is null)
                continue;

            pattern.Name = _localizationService[$"/CheatSheet/EncodingPattern/{pattern.Name}"];
        }

        return patterns;
    }

    private IEnumerable<IGrouping<string, InstructionMetadata>>? LoadInstructionSet(string filename, InstructionMetadata[] instructions)
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

    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of common instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, InstructionMetadata>? CommonInstructions { get; }
    
    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of floating-point instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, InstructionMetadata>? FloatInstructions { get; }
    
    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of coproc0 instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, InstructionMetadata>? CoProc0Instructions { get; }
    
    /// <summary>
    /// Gets an <see cref="ObservableGroupedCollection{String, InstructionMetadata}"/> of specialized instruction metadatas, grouped by category.
    /// </summary>
    public ObservableGroupedCollection<string, InstructionMetadata>? Specialized0Instructions { get; }

    /// <summary>
    /// Gets the list of general purpose registers.
    /// </summary>
    public GPRegister[] GPRegisters { get; }
}
