// Avishai Dernis 2026

using System;

namespace Zarem.Attributes.Arguments;

/// <summary>
/// A <see cref="ArgumentAttribute"/> for split arguments.
/// </summary>
public class SplitArgumentAttribute<TArg> : ArgumentAttribute
    where TArg : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitArgumentAttribute{TArg}"/> class.
    /// </summary>
    public SplitArgumentAttribute(string alias, TArg register, TArg imm) : base(alias)
    {
        RegisterArgument = register;
        ImmediateArgument = imm;
    }

    /// <summary>
    /// Gets the <typeparamref name="TArg"/> for the register component.
    /// </summary>
    public TArg RegisterArgument { get; }

    /// <summary>
    /// Gets the <typeparamref name="TArg"/> for the immediate component.
    /// </summary>
    public TArg ImmediateArgument { get; }
}
