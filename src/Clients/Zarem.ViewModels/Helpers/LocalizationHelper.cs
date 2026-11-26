// Avishai Dernis 2025

using System;
using Zarem.Services;

namespace Zarem.Helpers;

/// <summary>
/// A class with localization helper methods.
/// </summary>
public static class LocalizationHelper
{
    /// <summary>
    /// Localizes an enum display name.
    /// </summary>
    public static string LocalizeEnum<TEnum>(TEnum value)
        where TEnum : Enum
    {
        var key = $"{typeof(TEnum).Name}.{value}";

        var localizationService = Service.Get<ILocalizationService>();
        return localizationService[key];
    }

    /// <summary>
    /// Localizes an object display value.
    /// </summary>
    public static string Localize(object value)
    {
        var key = $"{value.GetType().Name}_{value}";

        var localizationService = Service.Get<ILocalizationService>();
        return localizationService[key];
    }
}
