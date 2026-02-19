// Avishai Dernis 2026

using System.Threading.Tasks;
using Zarem.Emulator;
using Zarem.Emulator.Interpreter;
using Zarem.Models.Files;
using Zarem.Services.Popup;
using Zarem.Services.Popup.Enums;
using Zarem.Services.Popup.Models;

namespace Zarem.Services;

/// <summary>
/// A service for handling emulation.
/// </summary>
public class DebugService : IDebugService
{
    private readonly IConsoleService _consoleService;
    private readonly IBuildService _buildService;
    private readonly ILocalizationService _localizationService;
    private readonly IPopupService _popupService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugService"/> class.
    /// </summary>
    public DebugService(IConsoleService consoleService, IBuildService buildService, ILocalizationService localizationService, IPopupService popupService, IProjectService projectService)
    {
        _consoleService = consoleService;
        _buildService = buildService;
        _localizationService = localizationService;
        _popupService = popupService;
        _projectService = projectService;
    }

    /// <inheritdoc/>
    public async Task RunAsync(bool debug = true)
    {
        // TODO: Report issue
        if (_projectService.Project is null)
            return;

        var buildResult = await _buildService.BuildProjectAsync(true);
        if (buildResult?.OutputModule is null)
            return;

        // Start a debug session
        var session = _projectService.Project.StartDebug(buildResult.OutputModule);
        if (session is null)
            return;

        // Cheat and grab the mips emulator
        if (session?.Emulator is not MIPSEmulator mipsEmu)
            return;

        _consoleService.ShowConsoleWindow();

        _ = new MARSTrapHandler(mipsEmu.Computer);

        session.Emulator.Start();
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
                var request = await _popupService.ShowPopAsync(popup);
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
                await _popupService.ShowPopAsync(popup);
                return false;
            }
        }

        return true;
    }
}
