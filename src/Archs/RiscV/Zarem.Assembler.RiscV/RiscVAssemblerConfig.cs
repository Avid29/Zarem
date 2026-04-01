// Avishai Dernis 2026

using Zarem.Assembler.Config;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Assembler;

/// <summary>
/// A class containing RISC-V assembler configuration info.
/// </summary>
public class RiscVAssemblerConfig(RiscVVersionInfo versionInfo) : AssemblerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVAssemblerConfig"/> class.
    /// </summary>
    public RiscVAssemblerConfig() : this(new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.General))
    {
    }

    /// <summary>
    /// Gets or sets the RISC-V version information to use for assembly.
    /// </summary>
    public RiscVVersionInfo VersionInfo { get; internal set; } = versionInfo;
}
