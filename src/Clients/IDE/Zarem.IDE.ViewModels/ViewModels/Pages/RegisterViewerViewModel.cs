// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IDispatcherService _dispatcherService;

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
    public ObservableCollection<IGrouping<string?, BindableRegister>> Registers { get; }

    /// <summary>
    /// Gets or sets whether or not the debugger is halted.
    /// </summary>
    public bool IsHalted
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    protected override void RegisterSession(DebugSession session)
    {
        var viewer = session.Debugger?.Viewer;
        if (viewer is null)
            return;

        var groups = viewer.RegisterViewer.Registers
            .Select(x => new BindableRegister(x, viewer.RegisterViewer))
            .GroupBy(x => x.RegisterMeta.Category);

        Registers.Clear();
        foreach ( var group in groups)
            Registers.Add(group);

        session.Debugger?.Halted += Debugger_Halted;
        session.Debugger?.Resumed += Debugger_Resumed;
    }

    /// <inheritdoc/>
    protected override void UnregisterSession()
    {
        Registers.Clear();
        Session?.Debugger?.Halted -= Debugger_Halted;
        Session?.Debugger?.Resumed -= Debugger_Resumed;
    }

    private void Debugger_Halted(Zebugger sender, ulong e)
    {
        _dispatcherService.RunOnUIThread(() =>
        {
            foreach (var reg in Registers.SelectMany(g => g.Select(x => x)))
            {
                reg.Invalidate();
            }

            IsHalted = true;
        });
    }

    private void Debugger_Resumed(object? sender, System.EventArgs e)
    {
        _dispatcherService.RunOnUIThread(() => IsHalted = false);
    }
}
