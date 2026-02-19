// Adam Dernis 2024

using Microsoft.UI.Xaml;
using System;

namespace Zarem.WinUI;

public partial class App
{
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the app current window.
    /// </summary>
    public Window? Window { get; private set; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services
    /// </summary>
    public IServiceProvider Services { get; }
}
