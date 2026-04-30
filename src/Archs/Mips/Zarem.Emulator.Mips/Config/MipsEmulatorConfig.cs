// Avishai Dernis 2026

using System.Xml.Serialization;
using Zarem.Emulator.Config.Enums;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Emulator.Config;

/// <summary>
/// A class containing emulator configurations for the MIPS emulator.
/// </summary>
public class MipsEmulatorConfig : EmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsEmulatorConfig"/> class.
    /// </summary>
    public MipsEmulatorConfig() : this(MipsVersion.Mips32R2)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsEmulatorConfig"/> class.
    /// </summary>
    public MipsEmulatorConfig(MipsVersion mipsVersion = MipsVersion.Mips32R2, ExecutionMode mode = ExecutionMode.Interpret)
    {
        Version = mipsVersion;
        ExecutionMode = mode;
    }

    /// <summary>
    /// Gets or sets the mips ISA version to emulate.
    /// </summary>
    [XmlIgnore]
    public MipsVersion Version { get; internal set; }

    /// <summary>
    /// Gets or sets whether or not to disable branch delay slot emulation
    /// </summary>
    public bool DisableDelaySlots { get; set; } = false;
}
