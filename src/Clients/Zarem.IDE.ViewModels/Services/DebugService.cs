// Avishai Dernis 2026

using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using Zarem.DebugSessions;
using Zarem.Emulator;
using Zarem.Emulator.Models.Enums;
using Zarem.Emulator.TrapHandlers;
using Zarem.IDE.Messages.DebugSession;
using Zarem.IDE.Models.Enums;
using Zarem.IDE.Services.Popup;
using Zarem.IDE.Services.Popup.Enums;
using Zarem.IDE.Services.Popup.Models;
using Zarem.Models.Files;

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
        ILocalizationService localizationService,
        IPopupService popupService,
        IProjectService projectService,
        IStateService stateService)
    {
        _messenger = messenger;
        _buildService = buildService;
        _consoleService = consoleService;
        _dispatcher = dispatcher;
        _localizationService = localizationService;
        _popupService = popupService;
        _projectService = projectService;
        _stateService = stateService;
    }

    /// <inheritdoc/>
    public async Task RunAsync(bool debug = true)
    {
        if (!_stateService.IsReady)
            return;

        if (_projectService.Project is null)
            return;

        var buildResult = await _buildService.BuildProjectAsync(false);
        if (buildResult?.OutputModule is null)
            return;

        // Start a debug session
        _session = _projectService.Project.StartDebug(buildResult.OutputModule);
        if (_session is null)
            return;

        // Cheat and grab the mips emulator
        if (_session?.Emulator is not MIPSEmulator mipsEmu)
            return;

        _consoleService.ShowConsoleWindow();

        _ = new MARSTrapHandler(mipsEmu.Computer);
        mipsEmu.StateChanged += MipsEmu_StateChanged;

        _stateService.SetState(IdeState.Running);
        _messenger.Send(new DebugSessionStartedMessage());
        _session.Emulator.Start();
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
        if (session?.Emulator is not MIPSEmulator mipsEmu)
            return;

        var trapHandler = new MARSTrapHandler(mipsEmu.Computer);

        session.Emulator.Start();
    }

    /// <inheritdoc/>
    public void StopDebugging()
    {
        if (_session?.Emulator.State is not (EmulatorState.Stopped or EmulatorState.Stopping))
            _session?.Emulator.ShutDown();
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

    private void MipsEmu_StateChanged(object? sender, EmulatorState e)
    {
        if (e is EmulatorState.Stopped)
        {
            _consoleService.HideConsoleWindow(_localizationService["DebugSessionEnded"]);

            _dispatcher.RunOnUIThread(() => _stateService.SetState(IdeState.Ready));
            _session = null;
        }
    }
}
