// Avishai Dernis 2026

using System.Xml.Serialization;
using Zarem.Models.Versioning;

namespace Zarem.Emulator.Config;

/// <summary>
/// A class containing emulator configurations for the RISC emulator.
/// </summary>
public class RiscVEmulatorConfig : EmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVEmulatorConfig"/> class.
    /// </summary>
    public RiscVEmulatorConfig() : this(new RiscVVersionInfo())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVEmulatorConfig"/> class.
    /// </summary>
    public RiscVEmulatorConfig(RiscVVersionInfo version)
    {
        VersionInfo = version;
    }

    /// <summary>
    /// Gets or sets the mips ISA version to emulate.
    /// </summary>
    [XmlIgnore]
    public RiscVVersionInfo VersionInfo { get; internal set; }
}
