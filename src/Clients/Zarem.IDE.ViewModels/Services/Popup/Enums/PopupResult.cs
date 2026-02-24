// Avishai Dernis 2025

namespace Zarem.Services.Popup.Enums;

/// <summary>
/// An enum describing the result of a popup request.
/// </summary>
public enum PopupResult
{
    /// <summary>
    /// The popup was closed without a selection.
    /// </summary>
    Closed,

    /// <summary>
    /// The primary button was selected.
    /// </summary>
    Primary,

    /// <summary>
    /// The secondary button was selected.
    /// </summary>
    Secondary,
}
