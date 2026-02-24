// Avishai Dernis 2025

using Zarem.Models.Enums;

namespace Zarem.Models;

/// <summary>
/// A record containing the compoenents of a dependencies details.
/// </summary>
public record ThirdPartyNotice(
    string DependencyName,
    string Url,
    LicenseType LicenseType,
    string? IconUrl = null);
