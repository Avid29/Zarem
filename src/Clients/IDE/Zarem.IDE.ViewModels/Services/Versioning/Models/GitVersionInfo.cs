// Avishai Dernis 2025

using System;

namespace Zarem.IDE.Services.Versioning.Models;

/// <summary>
/// A struct containing git version info.
/// </summary>
public record struct GitVersionInfo(string Commit, string Branch, string Sha, DateTime CommitDate);
