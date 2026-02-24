// Avishai Dernis 2024

using System.Collections.Generic;

namespace Zarem.IDE.Services;

/// <summary>
/// An interface for a localization service
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the localized <see langword="string"/> for a given resource.
    /// </summary>
    /// <param name="key">The key of the resource.</param>
    /// <returns>Localized <see langword="string"/> if valid, otherwise returns an empty <see langword="string"/>.</returns>
    string this[string key] { get; }

    /// <summary>
    /// Gets the localized <see langword="string"/> for a given resource.
    /// </summary>
    /// <param name="key">The key of the resource.</param>
    /// <param name="args">The arguments to use when formatting the string.</param>
    /// <returns>Localized <see langword="string"/> if valid, otherwise returns an empty <see langword="string"/>.</returns>
    string this[string key, params object[] args] { get; }

    /// <summary>
    /// Gets or sets the app's language override.
    /// </summary>
    string? LanguageOverride { get; set; }

    /// <summary>
    /// Gets a value indicating whether or not the current language is written right to left.
    /// </summary>
    bool IsRightToLeftLanguage { get; }

    /// <summary>
    /// Gets a value indicating whether or not the current language is the app's neutral language.
    /// </summary>
    bool IsNeutralLanguage { get; }

    /// <summary>
    /// Gets the list of available languages.
    /// </summary>
    IReadOnlyList<string> AvailableLanguages { get; }
}
