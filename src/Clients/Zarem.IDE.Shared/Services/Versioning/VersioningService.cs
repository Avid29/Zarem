// Avishai Dernis 2025

using System;
using Windows.ApplicationModel;
using Zarem.IDE.Services.Versioning.Models;

namespace Zarem.IDE.Services.Versioning;

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
            Build = packageVersion.Build,
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
