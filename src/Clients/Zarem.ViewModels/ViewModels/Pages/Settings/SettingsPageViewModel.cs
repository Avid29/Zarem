// Avishai Dernis 2025

using System.Collections.ObjectModel;
using Zarem.Services;
using Zarem.ViewModels.Pages.Abstract;

namespace Zarem.ViewModels.Pages.Settings;

/// <summary>
/// A view model for the settings page.
/// </summary>
public abstract class SettingsPageViewModelBase : PageViewModel
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPageViewModelBase"/> class.
    /// </summary>
    public SettingsPageViewModelBase(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/Settings"];

    /// <summary>
    /// Gets the collection of settings sub-pages.
    /// </summary>
    public abstract ObservableCollection<SettingsSubPageViewModel> SubPages { get; }
}
