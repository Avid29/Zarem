// Avishai Dernis 2026

using System;

namespace Zarem.Attributes;

/// <summary>
/// A <see cref="AssemblerArgumentAttribute"/> for register arguments.
/// </summary>
public class RegisterArgumentAttribute<TSet> : AssemblerArgumentAttribute
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
