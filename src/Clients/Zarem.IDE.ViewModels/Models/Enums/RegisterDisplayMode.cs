// Avishai Dernis 2026

namespace Zarem.IDE.Models.Enums;

/// <summary>
/// An enum for selecting a format to display register values.
/// </summary>
public enum RegisterDisplayMode
{
#pragma warning disable CS1591

    Decimal,
    Hex,

    /// <remarks>
    /// Waiting on https://github.com/microsoft/microsoft-ui-xaml/issues/2508 to implement.
    /// But that day is never coming, so someday I'll stop being lazy and write an actual converter.
    /// </remarks>
    LabelOffset,

#pragma warning restore CS1591
}
