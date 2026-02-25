// Avishai Dernis 2025

using System.Collections.Generic;
using Zarem.IDE.Models;
using Zarem.IDE.Models.Enums;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Versioning;
using Zarem.IDE.Services.Versioning.Models;
using Zarem.IDE.ViewModels.Pages.Abstract;

namespace Zarem.IDE.ViewModels.Pages;

/// <summary>
/// A view model for the about page.
/// </summary>
public class AboutPageViewModel : PageViewModel
{
    private ILocalizationService _localizationService;
    private readonly IVersioningService _versioningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutPageViewModel"/> class.
    /// </summary>
    public AboutPageViewModel(ILocalizationService localizationService, IVersioningService versioningService)
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
    public IEnumerable<ThirdPartyNotice> ThirdPartyNotices { get; } =
    [
        new("GitInfo", "https://github.com/devlooped/GitInfo", LicenseType.MIT),
        new("HexBox.WinUI", "https://github.com/hotkidfamily/HexBox.WinUI", LicenseType.MIT),
        new("LibObjectFile", "https://github.com/xoofx/LibObjectFile", LicenseType.MIT),
        new("Windows Community Toolkit", "https://github.com/CommunityToolkit/Windows", LicenseType.MIT),
        new("WinUIEdit", "https://github.com/BreeceW/WinUIEdit", LicenseType.Other),
    ];
}
