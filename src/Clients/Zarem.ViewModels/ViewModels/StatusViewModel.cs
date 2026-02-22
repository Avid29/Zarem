// Avishai Dernis 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.Messages.Build;
using Zarem.Models.Enums;
using Zarem.Services;

namespace Zarem.ViewModels;

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
            OnPropertyChanged(nameof(State));
            r.StatusMessage = m.Message;
        });
    }

    /// <summary>
    /// Gets the current build status.
    /// </summary>
    public IdeState State => _stateService.State;

    /// <summary>
    /// Gets the build status message.
    /// </summary>
    public string? StatusMessage
    {
        get;
        private set => SetProperty(ref field, value);
    }
}
