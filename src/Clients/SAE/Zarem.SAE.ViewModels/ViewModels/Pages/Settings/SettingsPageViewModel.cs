// Avishai Dernis 2025

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.ViewModels.Pages.Settings;

namespace Zarem.SAE.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the settings page.
/// </summary>
public class SettingsPageViewModel : ObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPageViewModel"/> class.
    /// </summary>
    public SettingsPageViewModel(ILocalizationService localizationService, ISettingsService settingsService, IVersioningService versioningService)
    {
        SubPages = [
                new AppSettingsViewModel(localizationService, settingsService, versioningService),
            ];
    }

    /// <summary>
    /// Gets the collection of settings sub-pages.
    /// </summary>
    public ObservableCollection<SettingsSubPageViewModel> SubPages { get; }
}
