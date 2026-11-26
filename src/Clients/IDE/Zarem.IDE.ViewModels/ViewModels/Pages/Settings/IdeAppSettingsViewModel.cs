// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.ViewModels.Pages.Settings;

namespace Zarem.IDE.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the app settings sub-page.
/// </summary>
public partial class IdeAppSettingsViewModel : AppSettingsViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdeAppSettingsViewModel"/> class.
    /// </summary>
    public IdeAppSettingsViewModel(ILocalizationService localizationService, ISettingsService settingsService, IVersioningService versioningService) : base(localizationService, settingsService, versioningService)
    {
    }

    /// <summary>
    /// Gets or sets the assembler language in settings.
    /// </summary>
    public string AssemblerLanguageOverride
    {
        get => _settingsService.Local.GetValue<string>(SettingsKeys.AssemblerLanguageOverride) ?? "app";
        set => _settingsService.Local.SetValue(SettingsKeys.AssemblerLanguageOverride, value is "app" ? null : value);
    }

    /// <summary>
    /// Gets or sets whether or not the app should restore open projects when opened.
    /// </summary>
    public bool RestoreOpenProject
    {
        get => _settingsService.Local.GetValue<bool>(SettingsKeys.RestoreOpenProject);
        set => _settingsService.Local.SetValue(SettingsKeys.RestoreOpenProject, value);
    }

    [RelayCommand]
    private void OpenAbout() => Service.Get<MainViewModel>().GoToPageByType<IdeAboutPageViewModel>();
}
