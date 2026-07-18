// Avishai Dernis 2026

using System.Collections.Generic;
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
        var instructionGroupsResources = resources.Where(x => x.EndsWith(".ig.json"));

        EncodingPatterns = LoadEncodingPatterns(assembly, encodingPatternResource);
    }

    /// <summary>
    /// Gets the localizer for the <see cref="CheatSheetPage"/>.
    /// </summary>
    public Localizer Localizer { get; }

    /// <summary>
    /// Gets the list of encoding pattern groups.
    /// </summary>
    public EncodingPatternGroup[]? EncodingPatterns { get; }
    
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
}
