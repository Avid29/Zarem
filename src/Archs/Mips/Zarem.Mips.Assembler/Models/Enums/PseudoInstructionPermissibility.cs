// Avishai Dernis 2025

namespace Zarem.Assembler.Models.Enums;

/// <summary>
/// An enum for describing the method used to determine permissibility of pseudo instructions.
/// </summary>
public enum PseudoInstructionPermissibility
{
    /// <summary>
    /// The set of pseudo-instructions describes which instructions are not allowed.
    /// </summary>
    Blacklist,

    /// <summary>
    /// The set of pseudo-instructions describes which instructions are allowed.
    /// </summary>
    Whitelist,
}
