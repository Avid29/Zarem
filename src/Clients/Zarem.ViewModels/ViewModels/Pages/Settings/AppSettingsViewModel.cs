// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Linq;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;

namespace Zarem.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the app settings sub-page.
/// </summary>
public partial class AppSettingsViewModel : SettingsSubPageViewModel
{
    #pragma warning disable CS1591
    protected readonly ILocalizationService _localizationService;
    protected readonly ISettingsService _settingsService;
    protected readonly IVersioningService _versioningService;
#pragma warning restore CS1591

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsViewModel"/> class.
    /// </summary>
    public AppSettingsViewModel(ILocalizationService localizationService, ISettingsService settingsService, IVersioningService versioningService)
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
        _versioningService = versioningService;
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/Settings/AppSettingsTitle"];

    /// <summary>
    /// Gets or sets the selected app theme.
    /// </summary>
    public Theme AppTheme
    {
        get => _settingsService.Local.GetValue<Theme>(Zarem.SettingsKeys.AppTheme);
        set => _settingsService.Local.SetValue(Zarem.SettingsKeys.AppTheme, value, notify: true);
    }

    /// <summary>
    /// Gets the list of available app theme options.
    /// </summary>
    public IEnumerable<Theme> AppThemeOptions => Enum.GetValues<Theme>();

    /// <summary>
    /// Gets or sets the app language in settings.
    /// </summary>
    public string AppLanguage
    {
        get => _settingsService.Local.GetValue<string>(SettingsKeys.LanguageOverride) ?? "system";
        set => _settingsService.Local.SetValue(SettingsKeys.LanguageOverride, value is "system" ? null : value);
    }

    /// <summary>
    /// Gets the list of available languages in the app.
    /// </summary>
    /// <remarks>
    /// "system" is a sentinel value since null and empty cannot be used in a ComboBox.
    /// </remarks>
    public IEnumerable<string> AppLanguageOptions => _localizationService.AvailableLanguages.Prepend("system");

    /// <summary>
    /// Gets the list of available languages for the assembler.
    /// </summary>
    /// <remarks>
    /// "app" is a sentinel value since null and empty cannot be used in a ComboBox.
    /// </remarks>
    public IEnumerable<string> AssemblerLanguageOptions => ["app", "en", "he"]; // TODO: Retrieve programmatically

    /// <summary>
    /// Gets the app's version.
    /// </summary>
    public string AppVersion =>
        _localizationService["/Settings/VersionFormat",
            _versioningService.AppVersion.MajorVersion,
            _versioningService.AppVersion.MinorVersion,
            _versioningService.AppVersion.Build];
}
