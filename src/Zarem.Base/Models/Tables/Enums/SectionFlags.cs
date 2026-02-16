// Avishai Dernis 2025

using System;

namespace Zarem.Models.Tables.Enums;

/// <summary>
/// An enum for accessibility
/// </summary>
[Flags]
public enum SectionFlags
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0x000,

    /// <summary>
    /// A flag for if the section is writable.
    /// </summary>
    Write = 0x001,

    /// <summary>
    /// A flag for if the section is loaded.
    /// </summary>
    Allocate = 0x002,

    /// <summary>
    /// A flag for if the section is executable.
    /// </summary>
    Execute = 0x004,

    /// <summary>
    /// A flag for if the section contains mergable data.
    /// </summary>
    Merge = 0x010,

    /// <summary>
    /// A flag for if the section consists of null-terminated strings.
    /// </summary>
    Strings = 0x020,

#pragma warning disable CS1591

    InfoLink = 0x040,
    LinkOrder = 0x080,
    OSNonConforming = 0x100,
    Group = 0x200,
    TLS = 0x400,
    Compressed = 0x800,

    Default = Allocate | Write | Execute,
#pragma warning restore CS1591
}
