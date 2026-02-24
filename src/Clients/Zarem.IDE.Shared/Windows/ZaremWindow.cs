// Avishai Dernis 2025

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Zarem.Services;
using Zarem.ViewModels;
using System;
using System.Runtime.InteropServices;

namespace Zarem.IDE.Windows;

public abstract partial class ZaremWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZaremWindow"/> class.
    /// </summary>
    public ZaremWindow()
    {
        ViewModel = App.Current.Services.GetRequiredService<WindowViewModel>();
    }

    protected void SetupWindowStyle()
    {
        // Set the app flow direction
        var localization = Service.Get<ILocalizationService>();
        if (localization.IsRightToLeftLanguage && Content is FrameworkElement fe)
        {
            // Set app content flow direction to RTL
            fe.FlowDirection = FlowDirection.RightToLeft;

            // Set window style to RTL
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            if (hwnd is 0)
                return;

            var exStyle = GetWindowLongPtr(hwnd, -20);
            exStyle |= 0x00400000;
            SetWindowLongPtr(hwnd, -20, exStyle);
        }
    }

    /// <summary>
    /// Gets the <see cref="WindowViewModel"/>.
    /// </summary>
    public WindowViewModel ViewModel { get; }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = false)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = false)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
}
