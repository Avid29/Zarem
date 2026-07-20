// Avishai Dernis 2024

using System.Xml.Serialization;
using Zarem.Linker.Config;
using Zarem.Mips.Models.Versioning;

namespace Zarem.Mips.Linker.Config;

/// <summary>
/// A class containing linker configurations.
/// </summary>
public class MipsLinkerConfig : LinkerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsLinkerConfig"/> class.
    /// </summary>
    public MipsLinkerConfig() : this(new())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsLinkerConfig"/> class.
    /// </summary>
    public MipsLinkerConfig(MipsVersionInfo version)
    {
        VersionInfo = version;
    }

    /// <summary>
    /// Gets or sets the mips version to assemble with.
    /// </summary>
    [XmlIgnore]
    public MipsVersionInfo VersionInfo { get; internal set; }
}
