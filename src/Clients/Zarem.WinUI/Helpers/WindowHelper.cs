// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using System.Collections.Generic;
using Zarem.WinUI.Windows;

namespace Zarem.WinUI.Helpers;

public static class WindowHelper
{
    private readonly static HashSet<ZaremWindow> _openWindows = [];

    /// <summary>
    /// Creates a new <see cref="ZaremWindow"/>.
    /// </summary>
    /// <typeparam name="T">The specific type of window created</typeparam>
    /// <returns>The <see cref="ZaremWindow"/> created.</returns>
    public static T CreateWindow<T>()
        where T : ZaremWindow, new()
    {
        var window = new T();
        TrackWindow(window);
        return window;
    }

    public static ZaremWindow? GetWindowForElement(UIElement element)
    {
        if (element.XamlRoot is null)
            return null;

        foreach (ZaremWindow window in _openWindows)
        {
            if (element.XamlRoot == window.Content.XamlRoot)
                return window;
        }

        return null;
    }

    private static void TrackWindow(ZaremWindow window)
    {
        window.Closed += (sender, args) =>
        {
            _openWindows.Remove(window);
        };

        _openWindows.Add(window);
    }
}
