// Avishai Dernis 2025

using Microsoft.UI.Xaml.Media;

namespace Zarem.IDE.Controls.CheatSheet.Palettes;

public record EncodingPatternSectionBrushPalette
{
    /// <summary>
    /// Gets or sets the brush used for the operation code in the encoding pattern display.
    /// </summary>
    public SolidColorBrush? OpCodeBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for the function code in the encoding pattern display.
    /// </summary>
    public SolidColorBrush? FuncCodeBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for the register function code in the encoding pattern display.
    /// </summary>
    public SolidColorBrush? RegisterFuncCodeBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for general-purpose registers in the encoding pattern display.
    /// </summary>
    public SolidColorBrush? GPRegisterBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for coprocessor registers in the encoding pattern display.
    /// </summary>
    public SolidColorBrush? CPRegisterBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for immediate values in the encoding pattern display.
    /// </summary>
    public SolidColorBrush? ImmediateValueBrush { get; set; }

    /// <summary>
    /// Gets or sets the brush used for formats in the encoding pattern display.
    /// </summary>
    public SolidColorBrush? FormatBrush { get; set; }
    
    public Brush? GeneralOrCoRegisterBrush {get; set; }
}
