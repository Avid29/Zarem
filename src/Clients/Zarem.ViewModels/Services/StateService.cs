// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zarem.Messages.Build;
using Zarem.Models.Enums;

namespace Zarem.Services;

/// <summary>
/// An implementation of the <see cref="IStateService"/>.
/// </summary>
public class StateService : IStateService
{
    private readonly IMessenger _messenger;

    private CancellationTokenSource? _resetToken;

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
    public bool IsReady =>
        State is IdeState.Ready or
        IdeState.BuildCompleted or IdeState.BuildFailed;

    /// <inheritdoc/>
    public bool IsRunning => State is IdeState.Running or IdeState.Debugging;

    /// <inheritdoc/>
    public void SetState(IdeState state, string? message = null)
    {
        _resetToken?.Cancel();

        // Update value and cache old value
        var old = State;
        State = state;

        // Check if the value actually changed.
        if (old != state)
        {
            _messenger.Send(new StateChangedMessage(state, message));
        }

        if (state is IdeState.BuildCompleted or IdeState.BuildFailed)
        {
            // Clear status after some time
            _ = WaitAndClearStatusAsync();
        }
    }

    private async Task WaitAndClearStatusAsync()
    {
        // Wait 5 seconds, then clear the status (unless cancelled)
        _resetToken = new CancellationTokenSource();
        var token = _resetToken.Token;
        await Task.Delay(TimeSpan.FromSeconds(5));
        if (token.IsCancellationRequested)
            return;

        SetState(IdeState.Ready);
    }
}
