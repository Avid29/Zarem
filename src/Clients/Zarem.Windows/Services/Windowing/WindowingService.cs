// Avishai Dernis 2025

using Zarem.Services.Windowing;
using Windows.UI.ViewManagement;

namespace Zarem.Windows.Services.Windowing;

/// <summary>
/// An implementation of the <see cref="IWindowingService"/>
/// </summary>
public class WindowingService : IWindowingService
{
    /// <inheritdoc/>
    public void ToggleFullScreen()
    {
        var view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
        {
            view.ExitFullScreenMode();
        }
        else
        {
            view.TryEnterFullScreenMode();
        }
    }
}
