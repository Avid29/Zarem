// Avishai Dernis 2025

using System.Threading.Tasks;
using Zarem.IDE.Services.Popup.Models;
using Zarem.IDE.Services.Popup.Enums;

namespace Zarem.IDE.Services.Popup;

/// <summary>
/// An interface for a service to create popups.
/// </summary>
public interface IPopupService
{
    /// <summary>
    /// Opens a popup simple popup and awaits a selection.
    /// </summary>
    /// <param name="title">The popup title.</param>
    /// <param name="description">The popup description.</param>
    /// <param name="localize">Whether or not to localize the title and description as keys.</param>
    Task<PopupResult> ShowPopupAsync(string title, string description, bool localize = true);

    /// <summary>
    /// Opens a popup and awaits a selection.
    /// </summary>
    Task<PopupResult> ShowPopupAsync(PopupDetails popup);

    /// <summary>
    /// Opens a popup and awaits a selection.
    /// </summary>
    Task<string?> ShowPopupAsync(TextInputPopupDetails popup);
}
