// Avishai Dernis 2026

using System;
using Zarem.Models.Versioning;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Attributes;

/// <summary>
/// An attribute for declaring a RISC-V extension.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class RiscVExtensionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVExtensionAttribute"/> class.
    /// </summary>
    public RiscVExtensionAttribute(string alias, RiscVExtensions misa = RiscVExtensions.None, RiscVZExtensions z = RiscVZExtensions.None)
    {
        Alias = alias;
        Dependencies = new RiscVExtensionInfo(misa, z);
    }

    /// <summary>
    /// Gets the alias of the extension.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Gets whether or not the extension is an MISA extension.
    /// </summary>
    public bool IsMisa => Alias.Length == 1 && char.IsAsciiLetter(Alias[0]);

    /// <summary>
    /// Gets the <see cref="RiscVExtensionInfo"/> of dependency extensions.
    /// </summary>
    public RiscVExtensionInfo Dependencies { get; }
}
