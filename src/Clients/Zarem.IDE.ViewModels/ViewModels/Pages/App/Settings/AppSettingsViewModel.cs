// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Settings.Enums;
using Zarem.Services.Versioning;

namespace Zarem.ViewModels.Pages.App.Settings;

/// <summary>
/// A view model for the app settings sub-page.
/// </summary>
public partial class AppSettingsViewModel : SettingsSubPageViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly IVersioningService _versioningService;

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
        get => _settingsService.Local.GetValue<Theme>(SettingsKeys.AppTheme);
        set => _settingsService.Local.SetValue(SettingsKeys.AppTheme, value, notify: true);
    }

    /// <summary>
    /// Gets the list of available app theme options.
    /// </summary>
    public IEnumerable<Theme> AppThemeOptions => Enum.GetValues<Theme>();

    /// <summary>
    /// Gets or sets the app language in settings.
    /// </summary>
    public string LanguageOverride
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
    /// Gets or sets whether or not the app should restore open projects when opened.
    /// </summary>
    public bool RestoreOpenProject
    {
        get => _settingsService.Local.GetValue<bool>(SettingsKeys.RestoreOpenProject);
        set => _settingsService.Local.SetValue(SettingsKeys.RestoreOpenProject, value);
    }

    /// <summary>
    /// Gets the app's version.
    /// </summary>
    public string AppVersion =>
        _localizationService["/Settings/VersionFormat",
            _versioningService.AppVersion.MajorVersion,
            _versioningService.AppVersion.MinorVersion,
            _versioningService.AppVersion.Build];

    [RelayCommand]
    private void OpenAbout() => Service.Get<MainViewModel>().GoToPageByType<AboutPageViewModel>();
}
