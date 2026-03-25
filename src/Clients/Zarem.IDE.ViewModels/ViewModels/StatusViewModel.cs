// Avishai Dernis 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics.Contracts;
using Zarem.IDE.Messages.Build;
using Zarem.IDE.Models.Enums;
using Zarem.IDE.Services;

namespace Zarem.IDE.ViewModels;

/// <summary>
/// A view model for the status bar.
/// </summary>
public class StatusViewModel : ObservableRecipient
{
    private readonly IMessenger _messenger;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusViewModel"/> class.
    /// </summary>
    public StatusViewModel(IMessenger messenger, IStateService stateService)
    {
        _messenger = messenger;
        _stateService = stateService;

        StatusMessage = string.Empty;
        IsActive = true;
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _messenger.Register<StatusViewModel, StateChangedMessage>(this, (r, m) =>
        {
            r.StatusMessage = m.Message;
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(IsReady));
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsPaused));
        });
    }

    /// <summary>
    /// Gets the current IDE state.
    /// </summary>
    public IdeState State => _stateService.State;

    /// <summary>
    /// Gets whether or not the current state is a ready state.
    /// </summary>
    public bool IsReady => _stateService.IsReady;

    /// <summary>
    /// Gets whether or not the current state is a running state.
    /// </summary>
    public bool IsRunning => _stateService.IsRunning;

    /// <summary>
    /// Gets whether or not the current state is paused.
    /// </summary>
    public bool IsPaused => _stateService.State is IdeState.Paused;

    /// <summary>
    /// Gets the build status message.
    /// </summary>
    public string? StatusMessage
    {
        get;
        private set => SetProperty(ref field, value);
    }
}
