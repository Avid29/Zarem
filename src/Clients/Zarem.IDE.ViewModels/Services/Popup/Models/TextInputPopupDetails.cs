// Avishai Dernis 2026

namespace Zarem.Services.Popup.Models;

/// <summary>
/// Details for a text input popup to display.
/// </summary>
public class TextInputPopupDetails : PopupDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextInputPopupDetails"/> class.
    /// </summary>
    public TextInputPopupDetails(string title, string? description = null) : base(title, description)
    {
    }

    /// <summary>
    /// Gets the popup's text validation regex.
    /// </summary>
    public string? ValidationRegex { get; init; }
}
