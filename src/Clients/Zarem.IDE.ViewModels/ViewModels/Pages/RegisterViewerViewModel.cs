// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Zarem.Debugger;
using Zarem.DebugSessions;
using Zarem.Helpers;
using Zarem.IDE.Bindables;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages.Abstract;

namespace Zarem.IDE.ViewModels.Pages;

/// <summary>
/// A view model for the register viewer.
/// </summary>
public class RegisterViewerViewModel : DebugPageViewModel
{
    private IDispatcherService _dispatcherService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewerViewModel"/> class.
    /// </summary>
    public RegisterViewerViewModel(IMessenger messenger, IDispatcherService dispatcherService) : base(messenger)
    {
        _dispatcherService = dispatcherService;

        Registers = [];
    }

    /// <inheritdoc/>
    public override string Title => "Register Viewer"; // TODO: Localization

    /// <summary>
    /// Gets the register viewer's symbol resolver.
    /// </summary>
    public SymbolResolver? SymbolResolver => Session?.SymbolResolver;

    /// <summary>
    /// Gets the collection of registers being viewed.
    /// </summary>
    public ObservableCollection<BindableRegister> Registers { get; }

    /// <inheritdoc/>
    protected override void RegisterSession(DebugSession session)
    {
        var viewer = session.Debugger?.Viewer;
        if (viewer is null)
            return;

        foreach (var reg in viewer.Registers.RegisterNames)
        {
            Registers.Add(new BindableRegister(reg, viewer.Registers));
        }

        session.Debugger?.Halted += Debugger_Halted;
    }

    /// <inheritdoc/>
    protected override void UnregisterSession()
    {
        Registers.Clear();
        Session?.Debugger?.Halted -= Debugger_Halted;
    }

    private void Debugger_Halted(Zebugger sender, ulong e)
    {
        _dispatcherService.RunOnUIThread(() =>
        {
            foreach (var reg in Registers)
            {
                reg.Invalidate();
            }
        });
    }
}
