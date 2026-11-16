// Avishai Dernis 2026

using System.IO;
using System.Threading.Tasks;
using Zarem.Emulator;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Services;
using Zarem.N64;
using Zarem.N64.Config;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Popup;
using Zarem.Services.Popup.Enums;
using Zarem.Services.Popup.Models;
using Zarem.Services.Settings;

namespace Zarem.SAE.Services;

/// <summary>
/// A service for handling emulation.
/// </summary>
public class EmulatorService : IEmulatorService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILocalizationService _localizationService;
    private readonly IPopupService _popupService;
    private readonly ISettingsService _settingsService;
    private Zaremulator? _emulator;
    private Nintendo64? _n64;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmulatorService"/> class.
    /// </summary>
    public EmulatorService(IFileSystemService fileSystemService, ILocalizationService localizationService, IPopupService popupService, ISettingsService settingsService)
    {
        _fileSystemService = fileSystemService;
        _localizationService = localizationService;
        _popupService = popupService;
        _settingsService = settingsService;
    }

    /// <inheritdoc/>
    public EmulatorState State => _emulator?.State ?? EmulatorState.Stopped;

    /// <inheritdoc/>
    public async Task InitializeEmulatorAsync()
    {
        // Retreive BIOS file
        var biosRomPath = _settingsService.Local.GetValue<string>(SettingsKeys.BiosRomPath);
        while (string.IsNullOrEmpty(biosRomPath) || !File.Exists(biosRomPath))
        {
            // Prompt the user to select a BIOS file
            var popup = new PopupDetails(_localizationService["/Popups/BiosNotFoundTitle"])
            {
                Description = _localizationService["/Popups/BiosNotFoundDesc"],
                PrimaryButtonText = _localizationService["/Popups/SelectCartridgeFile"],
                CloseButtonText = _localizationService["/Popups/Cancel"],
            };

            var result = await _popupService.ShowPopupAsync(popup);
            switch (result)
            {
                case PopupResult.Primary:
                    biosRomPath = await ConfigureBiosAsync();
                    break;
                case PopupResult.Closed:
                    return;
            }
        }


        var cartridgeRomPath = _settingsService.Local.GetValue<string>(SettingsKeys.CartridgeRomPath);
        while (string.IsNullOrEmpty(cartridgeRomPath) || !File.Exists(cartridgeRomPath))
        {
            var popup = new PopupDetails(_localizationService["/Popups/CartridgeSlotEmptyTitle"])
            {
                Description = _localizationService["/Popups/CartridgeSlotEmptyDesc"],
                PrimaryButtonText = _localizationService["/Popups/SelectCartridgeFile"],
                CloseButtonText = _localizationService["/Popups/Cancel"],
            };

            var result = await _popupService.ShowPopupAsync(popup);
            switch (result)
            {
                case PopupResult.Primary:
                    cartridgeRomPath = await InsertCartridgeAsync();
                    break;
                case PopupResult.Closed:
                    return;
            }
        }

        var config = new N64EmulatorConfig(biosRomPath, ExecutionMode.Interpret);
        _n64 = new Nintendo64(config);
        _n64.InsertCartridge(cartridgeRomPath);
        _emulator = new Zaremulator(_n64);

#if DEBUG
        ServiceCollection.DisassemblerService = new Mips.Disassembler.Services.MipsDisassemblerService(new(config.VersionInfo));
#endif
    }

    /// <inheritdoc/>
    public async Task StartEmulatorAsync()
    {
        if (_emulator is null)
        {
            await InitializeEmulatorAsync();
        }

        if (_emulator is null)
            return;

        _emulator.Reset();
        _emulator.Start();
    }

    /// <inheritdoc/>
    public void StopEmulator()
    {
        if (_emulator is null)
            return;

        _emulator.Shutdown();
    }

    /// <inheritdoc/>
    public async Task<string?> InsertCartridgeAsync()
    {
        var z64File = await _fileSystemService.PickFileAsync([".z64"]);
        if (z64File is null)
            return null;

        _settingsService.Local.SetValue(SettingsKeys.CartridgeRomPath, z64File.Path);
        return z64File.Path;
    }

    /// <inheritdoc/>
    public void EjectCartridge() => _settingsService.Local.SetValue<string?>(SettingsKeys.CartridgeRomPath, null);

    /// <inheritdoc/>
    public async Task<string?> ConfigureBiosAsync()
    {
        var biosRom = await _fileSystemService.PickFileAsync([".bin", ".rom"]);
        if (biosRom is null)
            return null;

        _settingsService.Local.SetValue(SettingsKeys.BiosRomPath, biosRom.Path);
        return biosRom.Path;
    }
}
