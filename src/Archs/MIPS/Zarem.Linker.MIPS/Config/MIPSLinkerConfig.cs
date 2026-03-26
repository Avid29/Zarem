// Avishai Dernis 2024

using System.Xml.Serialization;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Linker.Config;

/// <summary>
/// A class containing linker configurations.
/// </summary>
public class MipsLinkerConfig : LinkerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsLinkerConfig"/> class.
    /// </summary>
    public MipsLinkerConfig() : this(MipsVersion.Mips32R2)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsLinkerConfig"/> class.
    /// </summary>
    public MipsLinkerConfig(MipsVersion version = MipsVersion.Mips32R2)
    {
        MipsVersion = version;
    }

    /// <summary>
    /// Gets or sets the mips version to assemble with.
    /// </summary>
    [XmlIgnore]
    public MipsVersion MipsVersion { get; internal set; }
}
