// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Arguments;

/// <summary>
/// A base class for an attribute that describes how to parse an assembler argument.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public abstract class ArgumentAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    public ArgumentAttribute(string alias)
    {
        Alias = alias;
    }

    /// <summary>
    /// Gets the alias for the argument attribute.
    /// </summary>
    public string Alias { get; }
}
