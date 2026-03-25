// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using Zarem.DebugSessions;
using Zarem.IDE.Messages.DebugSessions;

namespace Zarem.IDE.ViewModels.Pages.Abstract;

/// <summary>
/// A base class for a <see cref="PageViewModel"/> that contains debug info.
/// </summary>
public abstract class DebugPageViewModel : PageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewerViewModel"/> class.
    /// </summary>
    public DebugPageViewModel(IMessenger messenger) : base(messenger)
    {
        IsActive = true;
    }

    /// <summary>
    /// Gets the active debug session.
    /// </summary>
    public DebugSession? Session { get; private set; }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        Messenger.Register<DebugPageViewModel, DebugSessionStartedMessage>(this, (r, m) =>
        {
            Session = m.Session;
            RegisterSession(m.Session);
        });
        Messenger.Register<DebugPageViewModel, DebugSessionEndedMessage>(this, (r, m) =>
        {
            UnregisterSession();
            Session = null;
        });
    }

    /// <summary>
    /// The event handler for a debug session starting.
    /// </summary>
    /// <param name="session">The debug session.</param>
    protected abstract void RegisterSession(DebugSession session);

    /// <summary>
    /// The event handler for a debug session ending.
    /// </summary>
    protected abstract void UnregisterSession();
}
