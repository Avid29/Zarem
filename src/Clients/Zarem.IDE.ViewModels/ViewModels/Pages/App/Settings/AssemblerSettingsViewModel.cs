// Avishai Dernis 2025

using Zarem.Services;
using Zarem.Services.Settings;
using System.Collections.Generic;

namespace Zarem.ViewModels.Pages.App.Settings;

/// <summary>
/// A view model for the assembler settings sub-page.
/// </summary>
public class AssemblerSettingsViewModel : SettingsSubPageViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsViewModel"/> class.
    /// </summary>
    public AssemblerSettingsViewModel(ILocalizationService localizationService, ISettingsService settingsService)
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/Settings/AssemblerSettingsTitle"];

    /// <summary>
    /// Gets or sets the assembler language in settings.
    /// </summary>
    public string AssemblerLanguageOverride
    {
        get => _settingsService.Local.GetValue<string>(SettingsKeys.AssemblerLanguageOverride) ?? "app";
        set => _settingsService.Local.SetValue(SettingsKeys.AssemblerLanguageOverride, value is "app" ? null : value);
    }

    /// <summary>
    /// Gets the list of available languages for the assembler.
    /// </summary>
    /// <remarks>
    /// "app" is a sentinel value since null and empty cannot be used in a ComboBox.
    /// </remarks>
    public IEnumerable<string> AssemblerLanguageOptions => ["app", "en", "he"]; // TODO: Retrieve programmatically
}
