// Avishai Dernis 2026

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Zarem.Config;
using Zarem.Emulator;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Disassembler.Services;
using Zarem.Mips.Services;
using Zarem.N64;
using Zarem.N64.Config;
using Zarem.SAE.Services.Settings;
using Zarem.Services.Settings;

namespace Zarem.SAE.Services;

/// <summary>
/// A service for handling emulation.
/// </summary>
public class EmulatorService : IEmulatorService
{
    private readonly ISettingsService _settingsService;
    private Zaremulator? _emulator;
    private Nintendo64? _n64;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmulatorService"/> class.
    /// </summary>
    public EmulatorService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <inheritdoc/>
    public EmulatorState State => _emulator?.State ?? EmulatorState.Stopped;

    /// <inheritdoc/>
    [MemberNotNull(nameof(_emulator), nameof(_n64))]
    public void InitializeEmulator()
    {
        var biosRomPath = _settingsService.Local.GetValue<string>(SettingsKeys.BiosRomPath);
        if (string.IsNullOrEmpty(biosRomPath) || !File.Exists(biosRomPath))
        {
            // TODO: Do a pop-up to ask the user to select a BIOS ROM file.
            throw new InvalidOperationException("BIOS ROM path is not set or the file does not exist.");
        }

        var config = new N64EmulatorConfig(biosRomPath, ExecutionMode.Interpret);
        _n64 = new Nintendo64(config);
        _emulator = new Zaremulator(_n64);

#if DEBUG
        ServiceCollection.DisassemblerService = new MipsDisassemblerService(new(config.VersionInfo));
#endif
    }

    /// <inheritdoc/>
    public void StartEmulator()
    {
        if (_emulator is null)
        {
            InitializeEmulator();
        }

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
}
