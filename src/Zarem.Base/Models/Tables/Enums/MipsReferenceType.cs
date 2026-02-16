// Adam Dernis 2024

namespace Zarem.Models.Tables.Enums;

/// <summary>
/// An enum for the type of references and relocations.
/// </summary>
public enum MipsReferenceType
{
    /// <summary>
    /// No relocation.
    /// </summary>
    None = 0,

    /// <summary>
    /// 16-bit half-word relocation.
    /// </summary>
    Absolute16 = 1,

    /// <summary>
    /// 32-bit full-word relocation.
    /// </summary>
    Absolute32 = 2,

    /// <summary>
    /// 32-bit PC-relative relocation.
    /// </summary>
    Relative32 = 3,

    /// <summary>
    /// 26-bit jump target location.
    /// </summary>
    JumpTarget26 = 4,

    /// <summary>
    /// High 16-bits of a 32-bit address.
    /// </summary>
    High16 = 5,

    /// <summary>
    /// Low 16-bits of a 32-bit address. 
    /// </summary>
    Low16 = 6,

    /// <summary>
    /// 16-bit offset relative to the global pointer register.
    /// </summary>
    GlobalRelative16 = 7,

    /// <summary>
    /// Reference to a literal in a literal pool offset from global pointer register.
    /// </summary>
    Literal = 8,

    /// <summary>
    /// 16-bit offset to a global offset table entry
    /// </summary>
    GlobalOffsetTable16 = 9,

    /// <summary>
    /// 16-bit PC-relative reference for brances.
    /// </summary>
    PCRelative16 = 10,

    /// <summary>
    /// 16-bit PC-relative call reference.
    /// </summary>
    Call16 = 11,

    /// <summary>
    /// 32-bit offset relative to the global pointer register.
    /// </summary>
    GlobalRelative32 = 12,

    /// <summary>
    /// 26-bit PC-relative reference for brances.
    /// </summary>
    PCRelative26 = 10,
}
