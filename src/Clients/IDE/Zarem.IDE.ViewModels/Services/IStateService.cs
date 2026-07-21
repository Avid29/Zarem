// Avishai Dernis 2026

using Zarem.IDE.Models.Enums;

namespace Zarem.IDE.Services;

/// <summary>
/// A service for managing the app state.
/// </summary>
public interface IStateService
{
    /// <summary>
    /// Gets or sets the IDE state.
    /// </summary>
    IdeState State { get; }

    /// <summary>
    /// Gets whether or not the IDE state is ready for an action.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Gets whether or not the IDE state is in a running state.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Sets the IDE state with a message.
    /// </summary>
    /// <param name="state">The new state of the IDE.</param>
    /// <param name="message">The message for the update.</param>
    void SetState(IdeState state, string? message = null);
}
