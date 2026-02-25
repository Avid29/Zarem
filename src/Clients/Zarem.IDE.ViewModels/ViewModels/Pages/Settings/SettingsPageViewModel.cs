// Avishai Dernis 2025

using System.Collections.ObjectModel;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Files;
using Zarem.IDE.Services.Settings;
using Zarem.IDE.Services.Versioning;
using Zarem.IDE.ViewModels.Pages.Abstract;

namespace Zarem.IDE.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the settings page.
/// </summary>
public class SettingsPageViewModel : PageViewModel
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPageViewModel"/> class.
    /// </summary>
    public SettingsPageViewModel(ILocalizationService localizationService, ISettingsService settingsService, IFileSystemService fileSystemService, IVersioningService versioningService)
    {
        _localizationService = localizationService;

        SubPages = [
                new AppSettingsViewModel(localizationService, settingsService, versioningService),
                new EditorSettingsViewModel(localizationService, settingsService, fileSystemService),
                new AssemblerSettingsViewModel(localizationService, settingsService)
            ];
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/Settings"];

    /// <summary>
    /// Gets the collection of settings sub-pages.
    /// </summary>
    public ObservableCollection<SettingsSubPageViewModel> SubPages { get; }
}
