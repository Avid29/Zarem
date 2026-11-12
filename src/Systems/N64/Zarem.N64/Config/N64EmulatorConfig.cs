// Avishai Dernis 2026

using Zarem.Emulator.Config.Enums;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Models.Versioning;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.N64.Config;

/// <summary>
/// An <see cref="MipsEmulatorConfig"/> for the N64 emulator.
/// </summary>
public class N64EmulatorConfig : MipsEmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64EmulatorConfig"/> class.
    /// </summary>
    public N64EmulatorConfig(ExecutionMode mode) : base(new MipsVersionInfo(MipsGeneration.MipsIII), mode)
    {
    }

    /// <summary>
    /// Gets or sets the path to the N64 boot ROM file.
    /// </summary>
    public string BootRomPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether or not the N64 emulator should simulate the presence of an Expansion Pak.
    /// </summary>
    public bool HasExpansionPak { get; set; } = true;
}
