// Avishai Dernis 2025

using Windows.UI.ViewManagement;
using WinRT.Interop;
using Zarem.Services.Windowing;

namespace Zarem.IDE.Services;

/// <summary>
/// An implementation of the <see cref="IWindowingService"/>
/// </summary>
public class WindowingService : IWindowingService
{
    /// <inheritdoc/>
    public nint GetWindowHandle() => WindowNative.GetWindowHandle(ZaremIDEApp.Current.Window);

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
