// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.ViewModels.Pages.Settings;

namespace Zarem.SAE.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the settings page.
/// </summary>
public partial class SaeSettingsPageViewModel : SettingsPageViewModelBase
{
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly IVersioningService _versioningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaeSettingsPageViewModel"/> class.
    /// </summary>
    public SaeSettingsPageViewModel(ILocalizationService localizationService, ISettingsService settingsService, IVersioningService versioningService) : base(localizationService)
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
        _versioningService = versioningService;
    }

    /// <inheritdoc/>
    protected override SettingsSubPageViewModel[] InitSubPages()
    {
        return [
            new SaeAppSettingsViewModel(_localizationService, _settingsService, _versioningService),
            ];
    }
}
