// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Messages.Build;
using Zarem.Services;
using Zarem.ViewModels.Pages.Abstract;

namespace Zarem.ViewModels.Pages;

/// <summary>
/// A view model for the error list.
/// </summary>
public class ErrorListViewModel : PageViewModel
{
    private readonly IMessenger _messenger;
    private readonly IDispatcherService _dispatcherService;
    private Logger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorListViewModel"/> class.
    /// </summary>
    public ErrorListViewModel(IMessenger messenger, IDispatcherService dispatcherService)
    {
        _messenger = messenger;
        _dispatcherService = dispatcherService;

        Messages = [];

        IsActive = true;
    }
    
    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _messenger.Register<ErrorListViewModel, BuildStartedMessage>(this, (r, m) => r.BeginLogging(m));
        _messenger.Register<ErrorListViewModel, BuildFinishedMessage>(this, (r, m) => r.EndLogging(m));
    }

    /// <inheritdoc/>
    public override string Title => "Error List"; // TODO: Localization

    /// <summary>
    /// Gets the list of errors, warnings, and messages.
    /// </summary>
    public ObservableCollection<ILog> Messages { get; }

    private void BeginLogging(BuildStartedMessage message)
    {
        Messages.Clear();

        _logger = message.Logger;
        _logger.EntryLogged += OnEntryLogged;
    }

    private void EndLogging(BuildFinishedMessage message)
    {
        if (_logger is null)
            return;

        _logger.EntryLogged -= OnEntryLogged;
        _logger = null;
    }

    private void OnEntryLogged(object? sender, ILog e)
    {
        _dispatcherService.RunOnUIThread(() =>
        {
            Messages.Add(e);
        });
    }
}
