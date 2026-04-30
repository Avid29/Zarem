// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Zarem.Debugger;
using Zarem.Debugger.Models.Enums;
using Zarem.DebugSessions;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.IDE.Messages.DebugSessions;
using Zarem.IDE.Models.Enums;
using Zarem.IDE.Services.Files;
using Zarem.IDE.Services.Popup;
using Zarem.IDE.Services.Popup.Enums;
using Zarem.IDE.Services.Popup.Models;
using Zarem.IDE.ViewModels;
using Zarem.IDE.ViewModels.Pages;
using Zarem.Models.Files;
using Zarem.Models.Tables;

namespace Zarem.IDE.Services;

/// <summary>
/// A service for handling emulation.
/// </summary>
public class DebugService : IDebugService
{
    private readonly IMessenger _messenger;
    private readonly IBuildService _buildService;
    private readonly IConsoleService _consoleService;
    private readonly IDispatcherService _dispatcher;
    private readonly IFileService _fileService;
    private readonly ILocalizationService _localizationService;
    private readonly IPopupService _popupService;
    private readonly IProjectService _projectService;
    private readonly IStateService _stateService;

    private DebugSession? _session;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugService"/> class.
    /// </summary>
    public DebugService(
        IMessenger messenger,
        IBuildService buildService,
        IConsoleService consoleService,
        IDispatcherService dispatcher,
        IFileService fileService,
        ILocalizationService localizationService,
        IPopupService popupService,
        IProjectService projectService,
        IStateService stateService)
    {
        _messenger = messenger;
        _buildService = buildService;
        _consoleService = consoleService;
        _dispatcher = dispatcher;
        _fileService = fileService;
        _localizationService = localizationService;
        _popupService = popupService;
        _projectService = projectService;
        _stateService = stateService;
    }

    /// <inheritdoc/>
    public SourceRange? ExecutingLocation
    {
        get => field;
        set
        {
            field = value;
            _dispatcher.RunOnUIThread(async () =>
            {
                // Open the file if possible
                if (value?.Start.File is not null)
                {
                    var file = await _fileService.GetFileAsync(value.Value.Start.File);
                    file?.Open();
                }

                _messenger.Send(new ExecutingLocationChangedMessage(value));
            });

            // TODO: Update the PC to match
        }
    }

    /// <inheritdoc/>
    public async Task RunAsync(bool debug = true)
    {
        if (!_stateService.IsReady)
            return;

        if (_projectService.Project is null)
            return;

        // TODO: We're rebuilding when debug is true because the debug info is not 
        // properly exported properly. This should be changed back to false when
        // issue #73 is addressed.
        var buildResult = await _buildService.BuildProjectAsync(debug);
        if (buildResult?.OutputModule is null)
            return;

        // Start a debug session
        _session = _projectService.Project.StartDebug(buildResult.OutputModule, debug);
        if (_session is null)
            return;

        _consoleService.ShowConsoleWindow();
        if (_session.Emulator.Computer.Devices.Any(x => x is IGraphicsDevice))
        {
            Service.Get<MainViewModel>().GoToPageByType<GraphicalOutputPageViewModel>();
        }

        _session.Emulator.StateChanged += Emulator_StateChanged;
        _session.Debugger?.Halted += Debugger_Halted;
        _session.Debugger?.Resumed += DebugService_Resumed;

        _stateService.SetState(IdeState.Running);
        _messenger.Send(new DebugSessionStartedMessage(_session));
        _session.Emulator.Start();
    }

    private void DebugService_Resumed(object? sender, EventArgs e)
    {
        _dispatcher.RunOnUIThread(() => _stateService.SetState(IdeState.Debugging));
        ExecutingLocation = null;
    }

    private void Debugger_Halted(Zebugger? sender, ulong e)
    {
        _dispatcher.RunOnUIThread(() => _stateService.SetState(IdeState.Paused));

        var location = _session?.LineResolver?.GetSourceLocation(e);
        if (location is null)
            return;

        ExecutingLocation = location;
    }

    /// <inheritdoc/>
    public async Task RunFileAsync(SourceFile file, bool debug = true)
    {
        // TODO: Report issue
        if (_projectService.Project is null)
            return;

        // Run checks for if the file should/can run
        bool shouldRun = await PreRunChecks(file);
        if (!shouldRun)
            return;

        var session = await _projectService.Project.StartDebugAsync(file.ObjectFile);

        // Cheat and grab the mips emulator
        if (session?.Emulator.Computer is not MipsComputer mipsComp)
            return;

        session.Emulator.Start();
    }

    /// <inheritdoc/>
    public void Continue() => Step(StepMode.Continue);

    /// <inheritdoc/>
    public void Step(StepMode mode)
    {
        if (_session?.Debugger is null)
            return;

        _session.Debugger.Step(mode);
    }

    /// <inheritdoc/>
    public void StopDebugging()
    {
        if (_session?.Emulator.State is not (EmulatorState.Stopped or EmulatorState.Stopping))
            _session?.Emulator.ShutDown();

        // Resume to ensure it reaches the shutdown
        Continue();
    }

    private async Task<bool> PreRunChecks(SourceFile file)
    {
        // Clean files can simply execute
        if (!file.IsDirty)
            return true;

        // Rebuild the file
        var buildResult = await _buildService.AssembleFilesAsync([file]);
        if (buildResult is null)
            return false;

        // If build failed, give a notice
        if (buildResult.FailedFiles.Count is not 0)
        {
            if (file.ObjectFile.Exists)
            {
                // Assembly failed, but an old build exists
                var title = _localizationService["/Popups/FileRunOldAssemblyTitle", file.Name];
                var popup = new PopupDetails(title)
                {
                    Description = _localizationService["/Popups/FileRunOldAssemblyDescription"],
                    PrimaryButtonText = _localizationService["/Popups/FileRunOldAssemblyPrimary"],
                    CloseButtonText = _localizationService["/Popups/Cancel"],
                };

                // Show the popup.
                // Cancel run if closed without primary button click
                var request = await _popupService.ShowPopupAsync(popup);
                if (request is PopupResult.Closed)
                    return false;
            }
            else
            {
                // Assembly failed and no old build exists
                var title = _localizationService["/Popups/FileAssemblyFailed", file.Name];
                var popup = new PopupDetails(title)
                {
                    CloseButtonText = _localizationService["/Popups/Okay"],
                };

                // Show the popup and return
                await _popupService.ShowPopupAsync(popup);
                return false;
            }
        }

        return true;
    }

    private void Emulator_StateChanged(object? sender, EmulatorState e)
    {
        if (e is EmulatorState.Stopped)
        {
            _dispatcher.RunOnUIThread(() =>
            {
                _stateService.SetState(IdeState.Ready);
                _messenger.Send(new DebugSessionEndedMessage());
            });
            _session?.Debugger?.Halted -= Debugger_Halted;
            _session?.Debugger?.Resumed -= DebugService_Resumed;
            _session?.Dispose();
            _session = null;

            _consoleService.HideConsoleWindow(_localizationService["DebugSessionEnded"]);
        }
    }
}
