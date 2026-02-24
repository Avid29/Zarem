// Avishai Dernis 2025

using Zarem.Services;
using System;

namespace Zarem.IDE.Helpers;

public static class LocalizationHelper
{
    public static string LocalizeEnum<TEnum>(TEnum value)
        where TEnum : Enum
    {
        var key = $"{typeof(TEnum).Name}.{value}";

        var localizationService = Service.Get<ILocalizationService>();
        return localizationService[key];
    }

    public static string Localize(object value)
    {
        var key = $"{value.GetType().Name}_{value}";

        var localizationService = Service.Get<ILocalizationService>();
        return localizationService[key];
    }
}
