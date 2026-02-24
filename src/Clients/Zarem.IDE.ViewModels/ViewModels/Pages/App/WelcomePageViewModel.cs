// Avishai Dernis 2025

using Zarem.Services;
using Zarem.ViewModels.Pages.Abstract;

namespace Zarem.ViewModels.Pages.App;

/// <summary>
/// A view model for the welcome page.
/// </summary>
public class WelcomePageViewModel : PageViewModel
{
    private ILocalizationService _localizationService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomePageViewModel"/> class.
    /// </summary>
    public WelcomePageViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/Welcome"];
}
