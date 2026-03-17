// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using Zarem.Debugger.Viewer;
using Zarem.DebugSessions;
using Zarem.IDE.Messages.Build;
using Zarem.IDE.Messages.DebugSessions;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages.Abstract;

namespace Zarem.IDE.ViewModels.Pages;

/// <summary>
/// A view model for the register viewer.
/// </summary>
public class RegisterViewerViewModel : PageViewModel
{
    private readonly IMessenger _messenger;
    private IDebugViewer? _viewer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewerViewModel"/> class.
    /// </summary>
    public RegisterViewerViewModel(IMessenger messenger)
    {
        _messenger = messenger;

        IsActive = true;
    }

    /// <inheritdoc/>
    public override string Title => "Register Viewer"; // TODO: Localization

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _messenger.Register<RegisterViewerViewModel, DebugSessionStartedMessage>(this, (r, m) => RegisterSession(m.Session));
        _messenger.Register<RegisterViewerViewModel, DebugSessionEndedMessage>(this, (r, m) => UnregisterSession());
    }

    private void RegisterSession(DebugSession session)
    {
        _viewer = session.Debugger?.Viewer;
        if (_viewer is null)
            return;


    }

    private void UnregisterSession()
    {
        _viewer = null;
    }
}
