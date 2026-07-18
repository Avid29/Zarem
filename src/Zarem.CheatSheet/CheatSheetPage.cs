// Avishai Dernis 2026

using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Zarem.CheatSheet.Models;
using Zarem.Localization;

namespace Zarem.CheatSheet;

/// <summary>
/// A model for a language's cheat sheet.
/// </summary>
public class CheatSheetPage
{
    private CheatSheetPage(Assembly assembly)
    {
        // Get localizer
        var @namespace = $"{assembly.GetName().Name}.Resources";
        Localizer = new Localizer(@namespace, assembly);

        // Get resource groups
        var resources = assembly.GetManifestResourceNames();
        var encodingPatternResource = resources.First(x => x.EndsWith("EncodingPatterns.json"));
        var instructionGroupsResources = resources.First(x => x.EndsWith("InstructionCollections.json"));

        EncodingPatternsGroups = LoadEncodingPatterns(assembly, encodingPatternResource);
        InstructionCollections = LoadInstructionSets(assembly, instructionGroupsResources);

        Localize();
    }

    /// <summary>
    /// Gets the localizer for the <see cref="CheatSheetPage"/>.
    /// </summary>
    public Localizer Localizer { get; }

    /// <summary>
    /// Gets the an array of instruction collections.
    /// </summary>
    public InstructionCollection[]? InstructionCollections { get; }

    /// <summary>
    /// Gets the list of encoding pattern groups.
    /// </summary>
    public EncodingPatternGroup[]? EncodingPatternsGroups { get; }
    
    /// <summary>
    /// Loads a cheatsheet from an assembly's resources.
    /// </summary>
    public static CheatSheetPage LoadCheatSheet(Assembly assembly) => new(assembly);

    private static Stream? LoadResource(Assembly assembly, string filename) =>
        assembly.GetManifestResourceStream(filename);

    private static EncodingPatternGroup[]? LoadEncodingPatterns(Assembly assembly, string resource)
    {
        using var stream = LoadResource(assembly, resource);
        if (stream is null)
            return null;

        var patterns = JsonSerializer.Deserialize<EncodingPatternGroup[]>(stream);
        if (patterns is null)
            return null;

        return patterns;
    }

    private static InstructionCollection[]? LoadInstructionSets(Assembly assembly, string resource)
    {
        using var stream = LoadResource(assembly, resource);
        if (stream is null)
            return null;

        var collections = JsonSerializer.Deserialize<InstructionCollection[]>(stream);
        if (collections is null)
            return null;

        return collections;
    }

    private void Localize()
    {
        // TODO: Improve localization system

        if (InstructionCollections is not null)
        {
            foreach (var collection in InstructionCollections)
            {
                collection.Name = Localizer[$"InstructionCollection/{collection.Name}"] ?? collection.Name;

                foreach (var group in collection.Groups)
                {
                    group.Name = Localizer[$"InstructionGroup/{group.Name}"] ?? group.Name;
                }
            }
        }

        if (EncodingPatternsGroups is not null)
        {
            foreach (var group in EncodingPatternsGroups)
            {
                group.Name = Localizer[$"EncodingGroup/{group.Name}"] ?? group.Name;

                foreach (var pattern in group.Patterns)
                {
                    pattern.Name = Localizer[$"EncodingPattern/{pattern.Name}"] ?? pattern.Name;

                    if (pattern.Sections is null)
                        continue;

                    foreach (var section in pattern.Sections)
                    {
                        if (section.Name is null)
                            continue;

                        section.Name = Localizer[$"EncodingSection/{section.Name}"] ?? section.Name;
                    }
                }
            }
        }
    }
}
