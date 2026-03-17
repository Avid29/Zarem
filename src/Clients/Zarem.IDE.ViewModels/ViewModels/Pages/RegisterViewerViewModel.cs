// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Zarem.Debugger.Viewer;
using Zarem.DebugSessions;
using Zarem.IDE.Bindables;
using Zarem.IDE.Messages.DebugSessions;
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
        Registers = [];

        IsActive = true;
    }

    /// <inheritdoc/>
    public override string Title => "Register Viewer"; // TODO: Localization

    /// <summary>
    /// Gets the collection of registers being viewed.
    /// </summary>
    public ObservableCollection<BindableRegister> Registers { get; }

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

        foreach (var reg in _viewer.Registers.RegisterNames)
        {
            Registers.Add(new BindableRegister(reg, _viewer.Registers));
        }
    }

    private void UnregisterSession()
    {
        _viewer = null;

        Registers.Clear();
    }
}
