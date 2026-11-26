// Avishai Dernis 2025

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
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly IFileSystemService _fileSystemService;
    private readonly IVersioningService _versioningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdeSettingsPageViewModel"/> class.
    /// </summary>
    public IdeSettingsPageViewModel(ILocalizationService localizationService, ISettingsService settingsService, IFileSystemService fileSystemService, IVersioningService versioningService) : base(localizationService)
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
        _fileSystemService = fileSystemService;
        _versioningService = versioningService;
    }

    /// <inheritdoc/>
    protected override SettingsSubPageViewModel[] InitSubPages()
    {
        return [
                new IdeAppSettingsViewModel(_localizationService, _settingsService, _versioningService),
                new EditorSettingsViewModel(_localizationService, _settingsService, _fileSystemService),
            ];
    }
}
