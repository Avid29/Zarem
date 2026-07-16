// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Register;

/// <summary>
/// An attribute that describes register.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class RegisterAttribute<TCategory> : Attribute
    where TCategory : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterAttribute{TCategory}"/> class.
    /// </summary>
    public RegisterAttribute(string alias, TCategory category)
    {
        Alias = alias;
        Category = category;
    }

    /// <summary>
    /// Gets or sets the register's alias.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Gets or sets the register's category.
    /// </summary>
    public TCategory Category { get; }
}
