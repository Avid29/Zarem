// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Register;

/// <summary>
/// An attribute that describes register.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class RegisterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterAttribute"/> class.
    /// </summary>
    public RegisterAttribute(string? alias)
    {
        Alias = alias;
    }

    /// <summary>
    /// Gets or sets the register's alias.
    /// </summary>
    public string? Alias { get; }
}
