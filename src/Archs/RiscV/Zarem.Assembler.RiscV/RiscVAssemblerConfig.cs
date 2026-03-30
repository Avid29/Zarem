// Avishai Dernis 2026

using Zarem.Assembler.Config;
using Zarem.Models.Versioning;

namespace Zarem.Assembler;

/// <summary>
/// A class containing RISC-V assembler configuration info.
/// </summary>
public class RiscVAssemblerConfig : AssemblerConfig
{
    /// <summary>
    /// Gets or sets the RISC-V version information to use for assembly.
    /// </summary>
    public RiscVVersionInfo VersionInfo { get; internal set; }
}
