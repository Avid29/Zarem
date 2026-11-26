// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;

/// <summary>
/// A struct represent the results of a float instruction's execution.
/// </summary>
public readonly struct MipsFloatExecution<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private MipsFloatExecution(MipsExecution<T> child)
    {
        ChildExecution = child;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsFloatExecution{T}"/> struct.
    /// </summary>
    public unsafe static MipsFloatExecution<T> CreateFloatWriteback<TFloat>(MipsFloatRegister dest, TFloat writeBack)
        where TFloat : unmanaged, INumber<TFloat>
    {
        long longValue = writeBack switch
        {
            int i => i,
            uint ui => ui,
            long l => l,
            ulong ul => (long)ul,
            float f => BitConverter.SingleToInt32Bits(f),
            double d => BitConverter.DoubleToInt64Bits(d),
            _ => long.CreateTruncating(writeBack),
        };

        if (sizeof(TFloat) == sizeof(float))
        {
            return new MipsFloatExecution<T>
            {
                Register = dest,
                Writeback = (int)longValue,
                Format = FloatWriteback.Word,
            };
        }
        else
        {
            return new MipsFloatExecution<T>
            {
                Register = dest,
                Writeback = longValue,
                Format = FloatWriteback.Long,
            };
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsFloatExecution{T}"/> struct.
    /// </summary>
    public static MipsFloatExecution<T> CreateControlWriteback(CP1CRegisters dest, uint writeBack)
    {
        return new MipsFloatExecution<T>
        {
            Register = (MipsFloatRegister)dest,
            Writeback = writeBack,
            Format = FloatWriteback.Control,
        };
    }

    /// <summary>
    /// Gets the child execution writeback info.
    /// </summary>
    public MipsExecution<T> ChildExecution { get; init; }

    /// <summary>
    /// Gets the writeback value.
    /// </summary>
    public long Writeback { get; init; }

    /// <summary>
    /// Gets the writeback register.
    /// </summary>
    public MipsFloatRegister Register { get; init; }

    /// <summary>
    /// Gets the writeback format.
    /// </summary>
    public FloatWriteback Format { get; init; }

    /// <summary>
    /// Casts a <see cref="MipsExecution{T}"/> to a <see cref="MipsFloatExecution{T}"/>.
    /// </summary>
    public static implicit operator MipsFloatExecution<T>(MipsExecution<T> value) => new(value);
}
