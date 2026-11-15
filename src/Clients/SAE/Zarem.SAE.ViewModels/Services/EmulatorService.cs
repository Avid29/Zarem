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
        var biosRomPath = _settingsService.Local.GetValue<string>(SettingsKeys.BiosRomPath);
        while (string.IsNullOrEmpty(biosRomPath) || !File.Exists(biosRomPath))
        {
            var popup = new PopupDetails(_localizationService["/Popups/BiosNotFoundTitle"])
            {
                Description = _localizationService["/Popups/BiosNotFoundDesc"],
                PrimaryButtonText = _localizationService["/Popups/SelectBiosFile"],
                CloseButtonText = _localizationService["/Popups/Okay"],
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

        var config = new N64EmulatorConfig(biosRomPath, ExecutionMode.Interpret);
        _n64 = new Nintendo64(config);
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
    public async Task<string?> ConfigureBiosAsync()
    {
        var biosRom = await _fileSystemService.PickFileAsync([".bin", ".rom"]);
        if (biosRom is null)
            return null;

        _settingsService.Local.SetValue(SettingsKeys.BiosRomPath, biosRom.Path);
        return biosRom.Path;
    }
}
