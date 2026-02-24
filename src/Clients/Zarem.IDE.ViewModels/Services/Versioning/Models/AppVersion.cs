// Avishai Dernis 2025

namespace Zarem.Services.Versioning.Models;

/// <summary>
/// A struct containing the app's version info.
/// </summary>
public readonly record struct AppVersion(ushort MajorVersion, ushort MinorVersion, ushort Revision, ushort Build);
