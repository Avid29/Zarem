// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors.Control;

/// <summary>
/// A struct represent the results of a float instruction's execution.
/// </summary>
public readonly struct MipsControlExecution<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private MipsControlExecution(MipsExecution<T> child)
    {
        ChildExecution = child;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsControlExecution<T> CreateTlbOperation(TlbOperation operation)
    {
        return new MipsControlExecution<T>
        {
            TlbOperation = operation,
        };
    }

    /// <summary>
    /// Gets the child execution writeback info.
    /// </summary>
    public MipsExecution<T> ChildExecution { get; init; }

    /// <summary>
    /// Gets the writeback register.
    /// </summary>
    public TlbOperation TlbOperation { get; init; }

    /// <summary>
    /// Casts a <see cref="MipsExecution{T}"/> to a <see cref="MipsControlExecution{T}"/>.
    /// </summary>
    public static implicit operator MipsControlExecution<T>(MipsExecution<T> value) => new(value);
}
