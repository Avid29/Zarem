// Avishai Dernis 2026

using Zarem.Models.Versioning.Enums;

namespace Zarem.Models.Versioning;

/// <summary>
/// A struct definig a RISC-V version, including the base ISA and supported extensions.
/// </summary>
public readonly struct RiscVVersion
{
    /// <summary>
    /// Gets the base RISC-V ISA version.
    /// </summary>
    public RiscVBaseVersion Base { get; }

    /// <summary>
    /// Gets the group of extensions in use.
    /// </summary>
    public RiscVExtensions Extensions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVVersion"/> struct.
    /// </summary>
    public RiscVVersion(RiscVBaseVersion @base, RiscVExtensions extensions)
    {
        Base = @base;
        Extensions = extensions | RiscVExtensions.Integers; // 'I' is always required
    }
}
