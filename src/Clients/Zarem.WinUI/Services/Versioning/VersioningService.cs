// Avishai Dernis 2025

using Zarem.Services.Versioning;
using Zarem.Services.Versioning.Models;
using System;
using Windows.ApplicationModel;

namespace Zarem.WinUI.Services.Versioning;

/// <summary>
/// An implementation of the <see cref="IVersioningService"/>.
/// </summary>
public class VersioningService : IVersioningService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersioningService"/> class.
    /// </summary>
    public VersioningService()
    {
        var packageVersion = Package.Current.Id.Version;
        AppVersion = new AppVersion
        {
            MajorVersion = packageVersion.Major,
            MinorVersion = packageVersion.Minor,
            Revision = packageVersion.Revision,
        };

        GitVersionInfo = new GitVersionInfo
        {
            Commit = ThisAssembly.Git.Commit,
            Branch = ThisAssembly.Git.Branch,
            Sha = ThisAssembly.Git.Sha,
            CommitDate = DateTime.Parse(ThisAssembly.Git.CommitDate),
        };
    }

    /// <inheritdoc/>
    public AppVersion AppVersion { get; }
    
    /// <inheritdoc/>
    public GitVersionInfo GitVersionInfo { get; }
}
