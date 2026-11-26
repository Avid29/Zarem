// Avishai Dernis 2025

using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.ViewModels.Pages.Settings;

namespace Zarem.IDE.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the app settings sub-page.
/// </summary>
public partial class SaeAppSettingsViewModel : AppSettingsViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaeAppSettingsViewModel"/> class.
    /// </summary>
    public SaeAppSettingsViewModel(ILocalizationService localizationService, ISettingsService settingsService, IVersioningService versioningService) : base(localizationService, settingsService, versioningService)
    {
    }
}
