// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct representing the results of an instruction's execution.
/// </summary>
public readonly struct RiscVExecution<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly T _secondary1;
    //private readonly ulong _secondary2;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVExecution{T}"/> struct.
    /// </summary>
    public static RiscVExecution<T> CreateWriteback(GPRegister dest, T writeback)
    {
        return new RiscVExecution<T>
        {
            WritebackGPRegister = dest,
            Writeback = writeback,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVExecution{T}"/> struct.
    /// </summary>
    public static RiscVExecution<T> CreateJump(T absolutePC)
    {
        return new RiscVExecution<T>
        {
            ProgramCounter = absolutePC,
        };
    }


    /// <summary>
    /// Gets the general purpose register destination of the output.
    /// </summary>
    /// <remarks>
    /// <see cref="GPRegister.Zero"/> if none.
    /// </remarks>
    public GPRegister WritebackGPRegister { get; init; }

    /// <summary>
    /// Gets the writeback value to the selected GPR register.
    /// </summary>
    public T Writeback { get; init; }

    /// <summary>
    /// Gets the type of secondary effect from the execution, if any.
    /// </summary>
    public SideEffect SideEffect { get; init; }

    /// <summary>
    /// Gets the new PC value, if applicable.
    /// </summary>
    public T ProgramCounter
    {
        get => _secondary1;
        init
        {
            _secondary1 = value;
            SideEffect = SideEffect.ProgramCounter;
        }
    }
}
