// Avishai Dernis 2026

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Zarem.IDE.Models.CheatSheet;
using Zarem.IDE.Services;

namespace Zarem.IDE.ViewModels.Pages.CheatSheet;

/// <summary>
/// A view model for the encoding pattern page.
/// </summary>
public class EncodingPatternsPageViewModel : CheatSheetSubPageViewModel
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncodingPatternsPageViewModel"/> class.
    /// </summary>
    public EncodingPatternsPageViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        PrimaryEncodingPatterns = new(LoadEncodingPatterns("PrimaryEncodings.json") ?? []);
        CoProcessor1Patterns = new(LoadEncodingPatterns("CoProcessor1Encodings.json") ?? []);
        CoProcessor0Patterns = new(LoadEncodingPatterns("CoProcessor0Encodings.json") ?? []);
        UniquePatterns = new(LoadEncodingPatterns("UniqueEncodings.json") ?? []);
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
}
