// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Register;

/// <summary>
/// An attribute that describes register category.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class RegisterSetAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterSetAttribute"/> class.
    /// </summary>
    public RegisterSetAttribute(string format, Type? setType = null, string? regex = null, int regCount = 0, int regOffset = 0)
    {
        Format = format;
        RegisterOffset = regOffset;
        RegisterCount = regCount;
        Regex = regex;
        SetType = setType;
    }

    /// <summary>
    /// Gets the format for creating an indexed register in the set.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Gets the starting index number of registers in the set.
    /// </summary>
    public int RegisterOffset { get; }

    /// <summary>
    /// Gets the number of registers in the set.
    /// </summary>
    public int RegisterCount { get; }

    /// <summary>
    /// Gets the <see cref="Type"/> for the register set's enum.
    /// </summary>
    public Type? SetType { get; }

    /// <summary>
    /// Gets the regex for identifying a register in the set.
    /// </summary>
    public string? Regex { get; }
}
