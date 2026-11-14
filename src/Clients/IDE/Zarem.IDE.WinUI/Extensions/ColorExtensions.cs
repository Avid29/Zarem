// Avishai Dernis 2025

using System;
using Windows.UI;

namespace Zarem.IDE.Extensions;

public static class ColorExtensions
{
    /// <summary>
    /// Applies an alpha blend between two colors.
    /// </summary>
    /// <remarks>
    /// Blends the overlay color over the base. The resulting opacity will be the same as the base's opacity.
    /// </remarks>
    /// <param name="base">The base color.</param>
    /// <param name="overlay">The overlay color.</param>
    /// <returns>The base color with the overlay color alpha blended in.</returns>
    public static Color AlphaBlend(this Color @base, Color overlay)
    {
        static byte Blend(byte over, byte @base, byte a)
        {
            double overCo = (double)a / 255;
            double baseCo = 1 - overCo;
            var blend = (over * overCo) + (@base * baseCo);
            return (byte)Math.Round(blend);
        }

        var r = Blend(overlay.R, @base.R, overlay.A);
        var g = Blend(overlay.G, @base.G, overlay.A);
        var b = Blend(overlay.B, @base.B, overlay.A);
        return Color.FromArgb(@base.A, r, g, b);
    }
}
