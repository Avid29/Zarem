// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Helpers;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.Interpret;

/// <summary>
/// A struct representing the results of an instruction's execution.
/// </summary>
public readonly struct RiscVExecution<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private const int REG_BITCOUNT = 5;

    private readonly T _secondary1;
    private readonly ulong _secondary2;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVExecution{T}"/> struct.
    /// </summary>
    public static RiscVExecution<T> CreateWriteback(RiscVGpRegister dest, T writeback)
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
    public unsafe static RiscVExecution<T> CreateFloatWriteback<TFloat>(RiscVFloatRegister dest, TFloat writeBack)
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
            return new RiscVExecution<T>
            {
                FloatReg = dest,
                FWordWriteBack = (int)longValue,
            };
        }
        else
        {
            return new RiscVExecution<T>
            {
                FloatReg = dest,
                FLongWriteBack = longValue,
            };
        }
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
    /// Initializes a new instance of the <see cref="RiscVExecution{T}"/> struct.
    /// </summary>
    public static RiscVExecution<T> CreateJumpAndLink(T absolutePC, T writeback, RiscVGpRegister dest = RiscVGpRegister.ReturnAddress)
    {
        return new RiscVExecution<T>
        {
            Writeback = writeback,
            WritebackGPRegister = dest,
            ProgramCounter = absolutePC,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVExecution{T}"/> struct.
    /// </summary>
    public static RiscVExecution<T> CreateMemRead(RiscVGpRegister dest, T address, int size, bool signed = true)
    {
        return new RiscVExecution<T>
        {
            WritebackGPRegister = dest,
            MemAddress = address,
            MemSize = (uint)size,
            SideEffect = signed ? RiscVSideEffect.ReadMemorySigned : RiscVSideEffect.ReadMemory,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVExecution{T}"/> struct.
    /// </summary>
    public static RiscVExecution<T> CreateMemWrite(T writeBack, T address, int size)
    {
        return new RiscVExecution<T>
        {
            Writeback = writeBack,
            MemAddress = address,
            MemSize = (uint)size,
            SideEffect = RiscVSideEffect.WriteMemory,
        };
    }

    /// <summary>
    /// Gets the general purpose register destination of the output.
    /// </summary>
    /// <remarks>
    /// <see cref="RiscVGpRegister.Zero"/> if none.
    /// </remarks>
    public RiscVGpRegister WritebackGPRegister { get; init; }

    /// <summary>
    /// Gets the writeback value to the selected GPR register.
    /// </summary>
    public T Writeback { get; init; }

    /// <summary>
    /// Gets the type of secondary effect from the execution, if any.
    /// </summary>
    public RiscVSideEffect SideEffect { get; init; }

    /// <summary>
    /// Gets the new PC value, if applicable.
    /// </summary>
    public T ProgramCounter
    {
        get => _secondary1;
        init
        {
            _secondary1 = value;
            SideEffect = RiscVSideEffect.ProgramCounter;
        }
    }

    /// <summary>
    /// Gets the memory address to read or write at, if applicable.
    /// </summary>
    public T MemAddress
    {
        get => _secondary1;
        init => _secondary1 = value;
    }

    /// <summary>
    /// Gets the size of the memory operation to perform, if applicable
    /// </summary>
    /// <remarks>
    /// Number of bytes to read/write.
    /// </remarks>
    public ulong MemSize
    {
        get => _secondary2;
        init => _secondary2 = value;
    }

    /// <summary>
    /// Gets the floating-point register for writeback.
    /// </summary>
    public RiscVFloatRegister FloatReg
    {
        get => (RiscVFloatRegister)byte.CreateTruncating(BitField.GetField(_secondary1, REG_BITCOUNT, 0));
        init => BitField.SetField(ref _secondary1, REG_BITCOUNT, 0, T.CreateTruncating((byte)value));
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="int"/>.
    /// </summary>
    public int FWordWriteBack
    {
        get => (int)FLongWriteBack;
        init
        {
            _secondary2 = (uint)value;
            SideEffect = RiscVSideEffect.WriteSingle;
        }
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="long"/>.
    /// </summary>
    public long FLongWriteBack
    {
        get => (long)_secondary2;
        init
        {
            _secondary2 = (ulong)value;
            SideEffect = RiscVSideEffect.WriteDouble;
        }
    }
}
