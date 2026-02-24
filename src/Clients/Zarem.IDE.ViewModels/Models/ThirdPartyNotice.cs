// Avishai Dernis 2025


// Avishai Dernis 2025

using Zarem.IDE.Models.Enums;

namespace Zarem.IDE.Models;

/// <summary>
/// A record containing the compoenents of a dependencies details.
/// </summary>
public record ThirdPartyNotice(
    string DependencyName,
    string Url,
    LicenseType LicenseType,
    string? IconUrl = null);
