// Avishai Dernis 2025

using System;

namespace Zarem.Services.Versioning.Models;

/// <summary>
/// A struct containing git version info.
/// </summary>
public record struct GitVersionInfo(string Commit, string Branch, string Sha, DateTime CommitDate);
