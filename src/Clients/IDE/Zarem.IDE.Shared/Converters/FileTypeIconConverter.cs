// Avishai Dernis 2025

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;

namespace Zarem.IDE.Converters;

/// <summary>
/// A converter that gets an <see cref="ImageSource"/> icon for a given file by name.
/// </summary>
public partial class FileTypeIconConverter : IValueConverter
{
    /// <summary>
    /// Gets the <see cref="ImageSource"/> to use for a *.asm file.
    /// </summary>
    public ImageSource? Assembly { get; set; }

    /// <summary>
    /// Gets the <see cref="ImageSource"/> to use for a *.obj file.
    /// </summary>
    public ImageSource? Object { get; set; }

    /// <summary>
    /// Gets the <see cref="ImageSource"/> to use for a *.obj file.
    /// </summary>
    public ImageSource? Project { get; set; }

    /// <summary>
    /// Gets the default image source to use for an unknown file type.
    /// </summary>
    public ImageSource? Default { get; set; }

    /// <inheritdoc/>
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string fileName)
            return Default;

        var type = Path.GetExtension(fileName);

        return type switch
        {
            ".asm" => Assembly,
            ".obj" => Object,
            ".zrmp" => Project,
            _ => Default,
        };
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, string language) => null;
}
