// Avishai Dernis 2025

namespace Zarem.Services.Windowing;

/// <summary>
/// An interface for a service that manages the app's open windows.
/// </summary>
public interface IWindowingService
{
    /// <summary>
    /// Toggles full screen for the current window.
    /// </summary>
    public void ToggleFullScreen();
}
