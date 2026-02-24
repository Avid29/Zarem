// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using Zarem.Models.EditorConfig.ColorScheme;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Settings;
using Zarem.Services.Settings.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zarem.ViewModels.Pages.App.Settings;

/// <summary>
/// A view model for the editor settings sub-page.
/// </summary>
public partial class EditorSettingsViewModel : SettingsSubPageViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly IFileSystemService _fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsViewModel"/> class.
    /// </summary>
    public EditorSettingsViewModel(ILocalizationService localizationService, ISettingsService settingsService, IFileSystemService fileSystemService)
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
        _fileSystemService = fileSystemService;

        EditorColorSchemeOptions = new(LoadEditorSchemes());
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/Settings/EditorSettingsTitle"];

    /// <summary>
    /// Gets or sets the real-time assembly setting option.
    /// </summary>
    public bool RealTimeAssembly
    {
        get => _settingsService.Local.GetValue<bool>(SettingsKeys.RealTimeAssembly);
        set
        {
            if (_settingsService.Local.SetValue(SettingsKeys.RealTimeAssembly, value, notify: true))
            {
                OnPropertyChanged(nameof(RealTimeAssembly));
            }
        }
    }

    /// <summary>
    /// Gets or sets the real-time assembly setting option.
    /// </summary>
    public AnnotationThreshold AnnotationThreshold
    {
        get => _settingsService.Local.GetValue<AnnotationThreshold>(SettingsKeys.AnnotationThreshold);
        set
        {
            if (_settingsService.Local.SetValue(SettingsKeys.AnnotationThreshold, value, notify: true))
            {
                OnPropertyChanged(nameof(AnnotationThreshold));
            }
        }
    }

    /// <summary>
    /// Gets the list of available annotation threshold options.
    /// </summary>
    public IEnumerable<AnnotationThreshold> AnnotationThresholdOptions => Enum.GetValues<AnnotationThreshold>();

    /// <summary>
    /// Gets or sets the editor color scheme.
    /// </summary>
    public EditorColorScheme? EditorColorScheme
    {
        get => _settingsService.Local.GetValue<EditorColorScheme>(SettingsKeys.EditorColorScheme);
        set
        {
            if (_settingsService.Local.SetValue(SettingsKeys.EditorColorScheme, value, notify: true))
            {
                OnPropertyChanged(nameof(EditorColorScheme));
            }
        }
    }

    /// <summary>
    /// Gets the list of available editor color schemes.
    /// </summary>
    public ObservableCollection<EditorColorScheme> EditorColorSchemeOptions { get; }

    private List<EditorColorScheme> LoadEditorSchemes()
    {
        // Get resources
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames().Where(x => x.StartsWith("Zarem.Resources.ColorSchemes"));

        // Extract editor schemes
        var editorSchemes = new List<EditorColorScheme>();
        foreach (var resource in resources)
        {
            // Load
            using Stream? stream = assembly.GetManifestResourceStream(resource);
            if (stream is null)
                continue;

            // Deserialize
            var editorColorScheme = JsonSerializer.Deserialize<EditorColorScheme>(stream);
            if (editorColorScheme is null)
                continue;

            editorSchemes.Add(editorColorScheme);
        }

        // Ensure current scheme is included
        var current = EditorColorScheme;
        if (current is not null && !editorSchemes.Any(x => x.Name == current.Name))
            editorSchemes.Add(current);

        return editorSchemes;
    }

    [RelayCommand]
    private async Task LoadSchemeFromFile()
    {
        // TODO: log errors

        // Attempt to pick a file
        var file = await _fileSystemService.PickFileAsync(".json");
        if (file is null)
            return;

        // Open file as a stream
        var stream = await file.OpenStreamForReadAsync();
        if (stream is null)
            return;

        // Attempt to deserialize file contents as a color scheme
        var scheme = await JsonSerializer.DeserializeAsync<EditorColorScheme>(stream);
        if (scheme is null)
            return;

        // Apply loaded scheme
        EditorColorSchemeOptions.Add(scheme);
        EditorColorScheme = scheme;
    }
}
