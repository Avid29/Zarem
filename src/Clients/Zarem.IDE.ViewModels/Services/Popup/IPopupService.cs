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
    /// Opens a popup and awaits a selection.
    /// </summary>
    Task<PopupResult> ShowPopupAsync(PopupDetails popup);

    /// <summary>
    /// Opens a popup and awaits a selection.
    /// </summary>
    Task<string?> ShowPopupAsync(TextInputPopupDetails popup);
}
