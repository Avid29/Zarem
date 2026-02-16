// Avishai Dernis 2026

using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Elf;
using Zarem.Elf.Config;
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
    private readonly IBuildService _buildService;
    private readonly ILocalizationService _localizationService;
    private readonly IPopupService _popupService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugService"/> class.
    /// </summary>
    public DebugService(IBuildService buildService, ILocalizationService localizationService, IPopupService popupService, IProjectService projectService)
    {
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

        // Start a debug session
        var session = _projectService.Project.StartDebug();
        if (session is null)
            return;

        session.Emulator.Start();
    }

    /// <inheritdoc/>
    public async Task RunFileAsync(SourceFile file, bool debug = true)
    {
        // TODO: Report issue
        if (_projectService.Project is null)
            return;

        // Run checks for if the file should/can run
        //bool shouldRun = await PreRunChecks(file);
        //if (!shouldRun)
        //    return;

        // TODO: Have the project build it based on the config

        // Cheat and build the file here
        using var readStream = File.OpenRead(file.FullPath);
        var result = await Assembler.Assembler.AssembleAsync(readStream, file.Name, new MIPSHandler(new()), new MIPSAssemblerConfig());
        if (result.Module is null)
            return;

        // Cheat and link here
        //var module = MIPSLinker.Link("entry", result.Module);
        //var elfModule = ElfModule.Create(module, new ElfConfig());
        //if (elfModule is null)
        //    return;

        // Start a debug session
        var session = _projectService.Project.StartDebug();
        if (session is null)
            return;

        // Cheat and grab the mips emulator
        if (session.Emulator is not MIPSEmulator mipsEmu)
            return;

        var trapHandler = new MARSTrapHandler(mipsEmu.Computer);

        //session.Emulator.Load(elfModule);
        session.Emulator.Start();
    }

    private async Task<bool> PreRunChecks(SourceFile file)
    {
        // Clean files can simply execute
        if (!file.IsDirty)
            return true;

        // Check if the file needs to be reassembled
        if (file.IsDirty)
        {
            // Rebuild the file
            await _buildService.AssembleFilesAsync([file]);
        }

        // If the file is still dirty, build failed
        if (file.IsDirty)
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
                // Assembly failedd and no old build exists
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
