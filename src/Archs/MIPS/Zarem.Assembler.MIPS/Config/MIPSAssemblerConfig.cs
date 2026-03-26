// Avishai Dernis 2024

using System.Collections.Generic;
using System.Xml.Serialization;
using Zarem.Assembler.Models.Enums;
using Zarem.Models.Instructions.Enums;

namespace Zarem.Assembler.Config;

/// <summary>
/// A class containing assembler configurations.
/// </summary>
public class MipsAssemblerConfig : AssemblerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsAssemblerConfig"/> class.
    /// </summary>
    public MipsAssemblerConfig() : this(MipsVersion.Mips32R2)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsAssemblerConfig"/> class.
    /// </summary>
    public MipsAssemblerConfig(MipsVersion version = MipsVersion.Mips32R2)
    {
        MipsVersion = version;
    }

    /// <summary>
    /// Gets or sets the mips version to assemble with.
    /// </summary>
    [XmlIgnore]
    public MipsVersion MipsVersion { get; internal set; }

    /// <summary>
    /// Gets whether the <see cref="PseudoInstructionSet"/> is a blacklist or whitelist.
    /// </summary>
    public PseudoInstructionPermissibility? PseudoInstructionPermissibility { get; set; }

    /// <summary>
    /// Gets the set of pseudo instructions to use as either a black or white list.
    /// </summary>
    public HashSet<string>? PseudoInstructionSet { get; set; } = null;
}
