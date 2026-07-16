// Avishai Dernis 2026

using System;

namespace Zarem.Attributes;

/// <summary>
/// A <see cref="AssemblerArgumentAttribute"/> for split arguments.
/// </summary>
public class SplitArgumentAttribute<TArg> : AssemblerArgumentAttribute
    where TArg : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitArgumentAttribute{TArg}"/> class.
    /// </summary>
    public SplitArgumentAttribute(TArg register, TArg imm)
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
