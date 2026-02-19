// Adam Dernis 2024

using System.Xml.Serialization;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Linker.Config;

/// <summary>
/// A class containing linker configurations.
/// </summary>
public class MIPSLinkerConfig : LinkerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSLinkerConfig"/> class.
    /// </summary>
    public MIPSLinkerConfig() : this(MipsVersion.MipsIII)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSLinkerConfig"/> class.
    /// </summary>
    public MIPSLinkerConfig(MipsVersion version = MipsVersion.MipsIII)
    {
        MipsVersion = version;
    }

    /// <summary>
    /// Gets or sets the mips version to assemble with.
    /// </summary>
    [XmlIgnore]
    public MipsVersion MipsVersion { get; internal set; }
}
