// Avishai Dernis 2024

using System.Collections.Generic;
using System.Xml.Serialization;
using Zarem.Assembler.Config;
using Zarem.Mips.Assembler.Models.Enums;
using Zarem.Mips.Models.Versioning;

namespace Zarem.Mips.Assembler;

/// <summary>
/// A class containing MIPS assembler configuration info.
/// </summary>
public class MipsAssemblerConfig : AssemblerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsAssemblerConfig"/> class.
    /// </summary>
    public MipsAssemblerConfig() : this(new MipsVersionInfo())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsAssemblerConfig"/> class.
    /// </summary>
    public MipsAssemblerConfig(MipsVersionInfo versionInfo)
    {
        VersionInfo = versionInfo;
    }

    /// <summary>
    /// Gets or sets the mips version to use for assembly.
    /// </summary>
    [XmlIgnore]
    public MipsVersionInfo VersionInfo { get; internal set; }

    /// <summary>
    /// Gets whether the <see cref="PseudoInstructionSet"/> is a blacklist or whitelist.
    /// </summary>
    public PseudoInstructionPermissibility? PseudoInstructionPermissibility { get; set; }

    /// <summary>
    /// Gets the set of pseudo instructions to use as either a black or white list.
    /// </summary>
    public HashSet<string>? PseudoInstructionSet { get; set; } = null;
}
