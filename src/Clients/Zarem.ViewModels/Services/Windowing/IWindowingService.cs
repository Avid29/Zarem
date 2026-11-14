// Avishai Dernis 2025

namespace Zarem.Services.Windowing;

/// <summary>
/// An interface for a service that manages the app's open windows.
/// </summary>
public interface IWindowingService
{
    /// <summary>
    /// Gets the Window handle.
    /// </summary>
    nint GetWindowHandle();

    /// <summary>
    /// Toggles full screen for the current window.
    /// </summary>
    void ToggleFullScreen();
}
