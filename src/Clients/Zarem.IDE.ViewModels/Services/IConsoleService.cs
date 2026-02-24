// Avishai Dernis 2026

namespace Zarem.IDE.Services;

/// <summary>
/// An interface for a service to interact with a console.
/// </summary>
public interface IConsoleService
{
    /// <summary>
    /// Requests to show the console window for the app.
    /// </summary>
    /// <returns><see langword="true"/> if the console was opened, <see langword="false"/> otherwise</returns>
    bool ShowConsoleWindow();

    /// <summary>
    /// Hides the console window.
    /// </summary>
    void HideConsoleWindow();

    /// <summary>
    /// Displays a message in the console, followed by an "Any key to close" message.
    /// </summary>
    void HideConsoleWindow(string message);
}
