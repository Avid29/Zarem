// Avishai Dernis 2026

using System;
using System.Runtime.InteropServices;
using Zarem.Services;

namespace Zarem.WinUI.Services;

/// <summary>
/// 
/// </summary>
public class ConsoleService : IConsoleService
{
    const int SwHide = 0;
    const int SwShow = 5;

    [DllImport(@"kernel32.dll", SetLastError = true)]
    static extern bool AllocConsole();

    [DllImport(@"kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport(@"user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <inheritdoc/>
    public bool ShowConsoleWindow()
    {
        var handle = GetConsoleWindow();

        if (handle == IntPtr.Zero)
        {
            return AllocConsole();
        }
        else
        {
            return ShowWindow(handle, SwShow);
        }
    }

    /// <inheritdoc/>
    public void HideConsoleWindow()
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SwHide);
    }
}
