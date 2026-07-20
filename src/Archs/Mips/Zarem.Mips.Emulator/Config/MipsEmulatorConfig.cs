// Avishai Dernis 2026

using System.Xml.Serialization;
using Zarem.Emulator.Config.Enums;
using Zarem.Mips.Models.Versioning;

namespace Zarem.Emulator.Config;

/// <summary>
/// A class containing emulator configurations for the MIPS emulator.
/// </summary>
public class MipsEmulatorConfig : EmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsEmulatorConfig"/> class.
    /// </summary>
    public MipsEmulatorConfig() : this(new())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsEmulatorConfig"/> class.
    /// </summary>
    public MipsEmulatorConfig(MipsVersionInfo mipsVersion, ExecutionMode mode = ExecutionMode.Interpret)
    {
        VersionInfo = mipsVersion;
        ExecutionMode = mode;
    }

    /// <summary>
    /// Gets or sets the mips ISA version to emulate.
    /// </summary>
    [XmlIgnore]
    public MipsVersionInfo VersionInfo { get; internal set; }

    /// <summary>
    /// Gets or sets whether or not to disable branch delay slot emulation
    /// </summary>
    public bool DisableDelaySlots { get; set; } = false;
}
