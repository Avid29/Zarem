// Avishai Dernis 2025

namespace Zarem.Services.Popup.Models;

/// <summary>
/// Details for a popup to display.
/// </summary>
public class PopupDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopupDetails"/> class.
    /// </summary>
    public PopupDetails(string title, string? description = null)
    {
        Title = title;
        Description = description;
    }

    /// <summary>
    /// Gets the popup's title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the popup's description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the popup's primary button's text.
    /// </summary>
    public string? PrimaryButtonText { get; init; }

    /// <summary>
    /// Gets the popup's secondary button's text.
    /// </summary>
    public string? SecondaryButtonText { get; init; }

    /// <summary>
    /// Gets the popup's close button's text.
    /// </summary>
    public required string CloseButtonText { get; init; }
}
