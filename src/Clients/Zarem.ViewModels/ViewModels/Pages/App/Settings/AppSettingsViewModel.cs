// Avishai Dernis 2025

using Zarem.Models;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Settings.Enums;
using Zarem.Services.Versioning;
using Zarem.Services.Versioning.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zarem.ViewModels.Pages.App.Settings;

/// <summary>
/// A view model for the app settings sub-page.
/// </summary>
public class AppSettingsViewModel : SettingsSubPageViewModel
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

    /// <summary>
    /// Gets the build's git version info.
    /// </summary>
    public GitVersionInfo GitInfo => _versioningService.GitVersionInfo;

    /// <summary>
    /// Gets a link to the build's commit on github.
    /// </summary>
    public string CommitGitHubLink => $"https://github.com/Avid29/Zarem/tree/{GitInfo.Sha}";

    /// <summary>
    /// Gets a list of third-party dependencies used in Zarem.
    /// </summary>
    public IEnumerable<ThirdPartyNotice> ThirdPartyNotices { get; } =
    [
        new("GitInfo", "https://github.com/devlooped/GitInfo"),
        new("HexBox.WinUI", "https://github.com/hotkidfamily/HexBox.WinUI"),
        new("LibObjectFile", "https://github.com/xoofx/LibObjectFile"),
        new("Windows Community Toolkit", "https://github.com/CommunityToolkit/Windows"),
        new("WinUIEdit", "https://github.com/BreeceW/WinUIEdit"),
    ];
}
