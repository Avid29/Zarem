// Avishai Dernis 2025

using System.Collections.Generic;
using Zarem.Models;
using Zarem.Services;
using Zarem.Services.Versioning;
using Zarem.Services.Versioning.Models;
using Zarem.ViewModels.Pages.Abstract;

namespace Zarem.ViewModels.Pages;

/// <summary>
/// A view model for the about page.
/// </summary>
public abstract class AboutPageViewModelBase : PageViewModel
{
#pragma warning disable CS1591
    protected readonly ILocalizationService _localizationService;
    protected readonly IVersioningService _versioningService;
#pragma warning restore CS1591

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutPageViewModelBase"/> class.
    /// </summary>
    public AboutPageViewModelBase(ILocalizationService localizationService, IVersioningService versioningService)
    {
        _localizationService = localizationService;
        _versioningService = versioningService;
    }

    /// <inheritdoc/>
    public override string Title => _localizationService["/PageTitles/About"];

    /// <summary>
    /// Gets the app's version.
    /// </summary>
    public string AppVersion =>
        _localizationService["/Settings/VersionFormat",
            _versioningService.AppVersion.MajorVersion,
            _versioningService.AppVersion.MinorVersion,
            _versioningService.AppVersion.Build];

    /// <summary>
    /// Gets the build's git version info.
    /// </summary>
    public GitVersionInfo GitInfo => _versioningService.GitVersionInfo;

    /// <summary>
    /// Gets a link to the build's commit on github.
    /// </summary>
    public string CommitGitHubLink => $"https://github.com/Avid29/Zarem/tree/{GitInfo.Sha}";

    /// <summary>
    /// Gets a list of third-party dependencies used in Zarem.
    /// </summary>
    public abstract IEnumerable<ThirdPartyNotice> ThirdPartyNotices { get; }
}
