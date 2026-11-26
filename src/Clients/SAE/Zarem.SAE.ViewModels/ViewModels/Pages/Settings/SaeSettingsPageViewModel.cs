// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Initializes a new instance of the <see cref="SaeSettingsPageViewModel"/> class.
    /// </summary>
    public SaeSettingsPageViewModel(ILocalizationService localizationService, ISettingsService settingsService, IVersioningService versioningService) : base(localizationService)
    {
        SubPages = [
            new SaeAppSettingsViewModel(localizationService, settingsService, versioningService),
            ];
    }

    /// <inheritdoc/>
    public override ObservableCollection<SettingsSubPageViewModel> SubPages { get; }
}
