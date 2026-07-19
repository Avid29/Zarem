// Avishai Dernis 2024

using Zarem.Attributes;

namespace Zarem.Mips.Models.Enums;

/// <summary>
/// An enum for the type of references and relocations.
/// </summary>
public enum MipsReferenceType : uint
{
    /// <summary>
    /// No relocation.
    /// </summary>
    None = 0,

    /// <summary>
    /// 16-bit half-word relocation.
    /// </summary>
    [ReferenceType(16)]
    Absolute16 = 1,

    /// <summary>
    /// 32-bit full-word relocation.
    /// </summary>
    [ReferenceType(32)]
    Absolute32 = 2,

    /// <summary>
    /// 32-bit PC-relative relocation.
    /// </summary>
    [ReferenceType(32)]
    Relative32 = 3,

    /// <summary>
    /// 26-bit jump target location.
    /// </summary>
    [ReferenceType(26)]
    JumpTarget26 = 4,

    /// <summary>
    /// High 16-bits of a 32-bit address.
    /// </summary>
    [ReferenceType("hi", 16, ShiftAmount = 16)]
    High16 = 5,

    /// <summary>
    /// Low 16-bits of a 32-bit address. 
    /// </summary>
    [ReferenceType("lo", 16)]
    Low16 = 6,

    /// <summary>
    /// 16-bit offset relative to the global pointer register.
    /// </summary>
    [ReferenceType(16)]
    GlobalRelative16 = 7,

    /// <summary>
    /// Reference to a literal in a literal pool offset from global pointer register.
    /// </summary>
    [ReferenceType(32)]
    Literal = 8,

    /// <summary>
    /// 16-bit offset to a global offset table entry
    /// </summary>
    [ReferenceType("got", 16)]
    GlobalOffsetTable16 = 9,

    /// <summary>
    /// 16-bit PC-relative reference for brances.
    /// </summary>
    [ReferenceType(16)]
    PCRelative16 = 10,

    /// <summary>
    /// 16-bit PC-relative call reference.
    /// </summary>
    [ReferenceType("call16", 16)]
    Call16 = 11,

    /// <summary>
    /// 32-bit offset relative to the global pointer register.
    /// </summary>
    [ReferenceType(32)]
    GlobalRelative32 = 12,

    /// <summary>
    /// 26-bit PC-relative reference for brances.
    /// </summary>
    [ReferenceType(26)]
    PCRelative26 = 13,
}
