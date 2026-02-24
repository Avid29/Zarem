// Avishai Dernis 2025

using Microsoft.UI.Xaml.Media;

namespace Zarem.IDE.Controls.CheatSheet.Palettes;

public record ArgumentBrushPalette
{
    /// <summary>
    /// Gets or sets the brush used for general-purpose registers in the usage pattern display.
    /// </summary>
    public SolidColorBrush? GPRegisterBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for coprocessor registers in the usage pattern display.
    /// </summary>
    public SolidColorBrush? CPRegisterBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for immediate values in the usage pattern display.
    /// </summary>
    public SolidColorBrush? ImmediateValueBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for formats in the usage pattern display.
    /// </summary>
    public SolidColorBrush? FormatBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for miscellaneous argument types in the usage pattern display.
    /// </summary>
    public SolidColorBrush? MiscArgBrush { get; set; }
}
