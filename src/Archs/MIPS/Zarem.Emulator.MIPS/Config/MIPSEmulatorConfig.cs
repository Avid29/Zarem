// Avishai Dernis 2026

using System.Xml.Serialization;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Emulator.Config;

/// <summary>
/// A class containing emulator configurations for the MIPS emulator.
/// </summary>
public class MIPSEmulatorConfig : EmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSEmulatorConfig"/> class.
    /// </summary>
    public MIPSEmulatorConfig() : this(MipsVersion.MipsIII)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSEmulatorConfig"/> class.
    /// </summary>
    public MIPSEmulatorConfig(MipsVersion mipsVersion = MipsVersion.MipsIII)
    {
        MipsVersion = mipsVersion;
    }

    /// <summary>
    /// Gets or sets the mips ISA version to emulate.
    /// </summary>
    [XmlIgnore]
    public MipsVersion MipsVersion { get; internal set; }

    /// <summary>
    /// Gets or sets whether or not to disable branch delay slot emulation
    /// </summary>
    public bool DisableBranchDelays { get; set; } = false;
}
