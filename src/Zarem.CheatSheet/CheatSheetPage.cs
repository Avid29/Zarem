// Avishai Dernis 2026

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Zarem.CheatSheet.Models;

namespace Zarem.CheatSheet;

/// <summary>
/// A model for a language's cheat sheet.
/// </summary>
public class CheatSheetPage
{
    private CheatSheetPage()
    {

    }

    /// <summary>
    /// Loads a cheatsheet from an assembly's resources.
    /// </summary>
    public static CheatSheetPage LoadCheatSheet(Assembly assembly)
    {
        // Get resource groups
        var resources = assembly.GetManifestResourceNames();
        var encodingPatternResources = resources.Where(x => x.EndsWith(".ep.json"));
        var instructionGroupsResources = resources.Where(x => x.EndsWith(".ig.json"));

        var patternsGroups = LoadEncodingPatterns(assembly, encodingPatternResources);

        return new CheatSheetPage();
    }

    private static Stream? LoadResource(Assembly assembly, string filename) =>
        assembly.GetManifestResourceStream(filename);

    private static List<EncodingPattern[]> LoadEncodingPatterns(Assembly assembly, IEnumerable<string> resources)
    {
        var patternGroups = new List<EncodingPattern[]>();
        foreach (var resource in resources)
        {
            using var stream = LoadResource(assembly, resource);
            if (stream is null)
                continue;

            var patterns = JsonSerializer.Deserialize<EncodingPattern[]>(stream);
            if (patterns is null)
                continue;

            patternGroups.Add(patterns);
        }

        return patternGroups;
    }
}
