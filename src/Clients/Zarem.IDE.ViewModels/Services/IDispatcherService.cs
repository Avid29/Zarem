// Avishai Dernis 2024

using System;

namespace Zarem.Services;

/// <summary>
/// An interface for a dispatcher service.
/// </summary>
public interface IDispatcherService
{
    /// <summary>
    /// Initializes the dispatcher.
    /// </summary>
    void Init();

    /// <summary>
    /// Runs an <see cref="Action"/> on the UI Thread.
    /// </summary>
    void RunOnUIThread(Action action);
}
