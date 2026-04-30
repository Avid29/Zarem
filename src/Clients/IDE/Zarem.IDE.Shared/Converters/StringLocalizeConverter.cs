// Avishai Dernis 2026

using Microsoft.UI.Xaml.Data;
using System;
using Zarem.IDE.Services;

namespace Zarem.IDE.Converters;

/// <summary>
/// An <see cref="IValueConverter"/> that localizes and formats strings.
/// </summary>
public partial class StringLocalizeConverter : IValueConverter
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizeConverter"/> class.
    /// </summary>
    public StringLocalizeConverter()
    {
        _localizationService = Service.Get<ILocalizationService>();
    }

    /// <inheritdoc/>
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        // Check if a format string was provided.
        if (parameter is not string format)
        {
            // Ensure the value is string key
            if (value is not string key)
                return null;

            // Use the value as a key for
            return _localizationService[key];
        }

        return _localizationService[format, value];
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
