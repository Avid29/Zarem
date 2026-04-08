// Avishai Dernis 2024

using System.Xml.Serialization;
using Zarem.Models.Versioning;

namespace Zarem.Linker.Config;

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
        Version = version;
    }

    /// <summary>
    /// Gets or sets the mips version to assemble with.
    /// </summary>
    [XmlIgnore]
    public RiscVVersionInfo Version { get; internal set; }
}
