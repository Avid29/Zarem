// Avishai Dernis 2024

using Microsoft.UI.Dispatching;
using Zarem.Services;
using System;

namespace Zarem.WinUI.Services;

/// <summary>
/// A <see cref="IDispatcherService"/> implementation for the windows client.
/// </summary>
public class DispatcherService : IDispatcherService
{
    private DispatcherQueue? _dispatcherQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatcherService"/> class.
    /// </summary>
    public DispatcherService()
    {
    }

    /// <inheritdoc/>
    public void Init()
    {
        // TODO: This won't work with multi-window
        _dispatcherQueue = App.Current.Window?.DispatcherQueue;
    }

    /// <inheritdoc/>
    public void RunOnUIThread(Action action)
    {
        _dispatcherQueue?.TryEnqueue(action.Invoke);
    }
}
