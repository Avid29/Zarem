// Avishai Dernis 2024

using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Zarem.Services;

namespace Zarem.WinUI.Services;

/// <summary>
/// A service that retrieves localization details.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private ResourceLoader? _loader;

    private ResourceLoader Loader => _loader ??= ResourceLoader.GetForViewIndependentUse();

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class.
    /// </summary>
    public LocalizationService()
    {
        UpdateLanguage(LanguageOverride);
    }

    /// <inheritdoc/>
    public string this[string key] => Loader.GetString(key);
    
    /// <inheritdoc/>
    public string this[string key, params object[] args] => string.Format(this[key], args);

    /// <inheritdoc/>
    public bool IsRightToLeftLanguage
    {
        get
        {
            string flowDirectionSetting = ResourceContext.GetForViewIndependentUse().QualifierValues["LayoutDirection"];
            return flowDirectionSetting == "RTL";
        }
    }

    /// <inheritdoc/>
    public string? LanguageOverride
    {
        get => ApplicationLanguages.PrimaryLanguageOverride;
        set => UpdateLanguage(value);
    }

    /// <inheritdoc/>
    public bool IsNeutralLanguage => CultureInfo.CurrentCulture.Name == "en-US";

    /// <inheritdoc/>
    public IReadOnlyList<string> AvailableLanguages => ApplicationLanguages.ManifestLanguages;

    private void UpdateLanguage(string? code)
    {
        ApplicationLanguages.PrimaryLanguageOverride = code;
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture =
            code is null ? CultureInfo.InstalledUICulture : new CultureInfo(code);
    }
}
