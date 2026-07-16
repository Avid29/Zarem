// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Register;

/// <summary>
/// An attribute that describes register.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class RegisterAttribute<TCategory> : RegisterAttribute
    where TCategory : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterAttribute{TCategory}"/> class.
    /// </summary>
    public RegisterAttribute(string alias, TCategory category) : base(alias)
    {
        Category = category;
    }

    /// <summary>
    /// Gets or sets the register's category.
    /// </summary>
    public TCategory Category { get; }
}
