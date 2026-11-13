// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Zarem.Emulator;
using Zarem.Emulator.Config.Enums;
using Zarem.Mips.Disassembler.Services;
using Zarem.Mips.Services;
using Zarem.N64;
using Zarem.N64.Config;
using Zarem.SAE.Services.Settings;
using Zarem.Services.Files;
using Zarem.Services.Settings;

namespace Zarem.SAE.ViewModels;

/// <summary>
/// The main view model for the application.
/// </summary>
public partial class MainViewModel : ObservableRecipient
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel(IFileSystemService fileSystemService, ISettingsService settingsService)
    {
        _fileSystemService = fileSystemService;
        _settingsService = settingsService;
    }

    /// <summary>
    /// Gets the Nintendo 64 emulator instance.
    /// </summary>
    public Nintendo64? N64 { get; private set; }

    /// <summary>
    /// Initializes  
    /// </summary>
    public async Task InitializeEmulatorsAsync()
    {
        var biosRomPath = _settingsService.Local.GetValue<string>(SettingsKeys.BiosRomPath);

        while (string.IsNullOrEmpty(biosRomPath) || !File.Exists(biosRomPath))
        {
            var biosRom = await _fileSystemService.PickFileAsync([".bin", ".rom"]);
            biosRomPath = biosRom?.Path;
            _settingsService.Local.SetValue(SettingsKeys.BiosRomPath, biosRomPath);
        }

        var config = new N64EmulatorConfig(biosRomPath, ExecutionMode.Interpret);
        N64 = new Nintendo64(config);

#if DEBUG
        ServiceCollection.DisassemblerService = new MipsDisassemblerService(new(config.VersionInfo));
#endif

        var emulator = new Zaremulator(N64);
        emulator.Reset();
        emulator.Start();
    }
}
