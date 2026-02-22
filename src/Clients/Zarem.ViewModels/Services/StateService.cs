// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using Zarem.Messages.Build;
using Zarem.Models.Enums;

namespace Zarem.Services;

/// <summary>
/// An implementation of the <see cref="IStateService"/>.
/// </summary>
public class StateService : IStateService
{
    private readonly IMessenger _messenger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateService"/> class.
    /// </summary>
    public StateService(IMessenger messenger)
    {
        _messenger = messenger;
    }

    /// <inheritdoc/>
    public IdeState State { get; private set; }

    /// <inheritdoc/>
    public bool Ready =>
        State is IdeState.Ready or
        IdeState.BuildCompleted or IdeState.BuildFailed;

    /// <inheritdoc/>
    public void SetState(IdeState state, string? message = null)
    {
        // Update value and cache old value
        var old = State;
        State = state;

        // Check if the value actually changed.
        if (old != state)
        {
            _messenger.Send(new StateChangedMessage(state, message));
        }
    }
}
