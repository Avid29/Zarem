// Avishai Dernis 2025

using System.Collections.ObjectModel;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Settings;
using Zarem.Services.Versioning;
using Zarem.ViewModels.Pages.Settings;

namespace Zarem.IDE.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the settings page.
/// </summary>
public class IdeSettingsPageViewModel : SettingsPageViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdeSettingsPageViewModel"/> class.
    /// </summary>
    public IdeSettingsPageViewModel(ILocalizationService localizationService, ISettingsService settingsService, IFileSystemService fileSystemService, IVersioningService versioningService) : base(localizationService)
    {
        SubPages = [
                new IdeAppSettingsViewModel(localizationService, settingsService, versioningService),
                new EditorSettingsViewModel(localizationService, settingsService, fileSystemService),
            ];
    }

    /// <inheritdoc/>
    public override ObservableCollection<SettingsSubPageViewModel> SubPages { get; }
}
