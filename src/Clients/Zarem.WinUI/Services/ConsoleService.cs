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
    private readonly ILocalizationService _localizationService;

    const int SwHide = 0;
    const int SwShow = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleService"/> class.
    /// </summary>
    /// <param name="localizationService"></param>
    public ConsoleService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

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
            Console.Clear();
            return ShowWindow(handle, SwShow);
        }
    }

    /// <inheritdoc/>
    public void HideConsoleWindow()
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SwHide);
    }

    /// <inheritdoc/>
    public void HideConsoleWindow(string message)
    {
        // Write the message, followed by closing instructions
        Console.WriteLine(message);
        Console.WriteLine(_localizationService["PressAnyKeyCloseConsole"]);

        // Wait for input and hide the console.
        Console.ReadKey();
        HideConsoleWindow();
    }
}
