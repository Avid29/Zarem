// Avishai Dernis 2026

using System.IO;
using Zarem.Models.Enums;

namespace Zarem.Services;

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
    bool Ready { get; }

    /// <summary>
    /// Sets the IDE state with a message.
    /// </summary>
    /// <param name="state">The new state of the IDE.</param>
    /// <param name="message">The message for the update.</param>
    void SetState(IdeState state, string? message = null);
}
