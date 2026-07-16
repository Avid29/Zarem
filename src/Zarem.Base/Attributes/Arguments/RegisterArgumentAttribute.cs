// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Arguments;

/// <summary>
/// A <see cref="ArgumentAttribute"/> for register arguments.
/// </summary>
public class RegisterArgumentAttribute<TSet> : ArgumentAttribute
    where TSet : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterArgumentAttribute{TSet}"/> class.
    /// </summary>
    public RegisterArgumentAttribute(TSet set)
    {
        RegisterSet = set;
    }

    /// <inheritdoc/>
    public TSet RegisterSet { get; }
}
