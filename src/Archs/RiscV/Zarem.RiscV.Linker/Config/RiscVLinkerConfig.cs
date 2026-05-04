// Avishai Dernis 2024

using System.Xml.Serialization;
using Zarem.Linker.Config;
using Zarem.Models.Versioning;

namespace Zarem.RiscV.Linker.Config;

/// <summary>
/// A class containing linker configurations.
/// </summary>
public class RiscVLinkerConfig : LinkerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVLinkerConfig"/> class.
    /// </summary>
    public RiscVLinkerConfig() : this(new RiscVVersionInfo())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVLinkerConfig"/> class.
    /// </summary>
    public RiscVLinkerConfig(RiscVVersionInfo version)
    {
        VersionInfo = version;
    }

    /// <summary>
    /// Gets or sets the mips version to assemble with.
    /// </summary>
    [XmlIgnore]
    public RiscVVersionInfo VersionInfo { get; internal set; }
}
