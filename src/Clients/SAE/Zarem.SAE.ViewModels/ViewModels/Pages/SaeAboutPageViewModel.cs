// Avishai Dernis 2025

using System.Collections.Generic;
using Zarem.Models;
using Zarem.Models.Enums;
using Zarem.Services;
using Zarem.Services.Versioning;
using Zarem.ViewModels.Pages;

namespace Zarem.SAE.ViewModels.Pages;

/// <summary>
/// A view model for the about page.
/// </summary>
public class SaeAboutPageViewModel : AboutPageViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaeAboutPageViewModel"/> class.
    /// </summary>
    public SaeAboutPageViewModel(ILocalizationService localizationService, IVersioningService versioningService) : base(localizationService, versioningService)
    {
    }

    /// <summary>
    /// Gets a list of third-party dependencies used in Zarem.
    /// </summary>
    public override IEnumerable<ThirdPartyNotice> ThirdPartyNotices { get; } =
    [
        new("GitInfo", "https://github.com/devlooped/GitInfo", LicenseType.MIT),
        new("Windows Community Toolkit", "https://github.com/CommunityToolkit/Windows", LicenseType.MIT),
    ];
}
