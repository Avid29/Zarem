// Avishai Dernis 2025

using System;
using System.Numerics;
using Zarem.Emulator.Models.Enums;
using Zarem.Helpers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct representing the results of an instruction's execution.
/// </summary>
public readonly struct MipsExecution<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private const int REG_BITCOUNT = 5;

    // These values are used for secondary effects
    // They can be (low, high), (memAddress, size*(-signed)), (pc, _), (writeback, register|regset)
    private readonly T _secondary1;
    private readonly ulong _secondary2;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateWriteback(GPRegister dest, T writeBack)
    {
        return new MipsExecution<T>
        {
            GPR = dest,
            WriteBack = writeBack,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateWriteback(CP0Registers dest, T writeBack)
    {
        return new MipsExecution<T>
        {
            CoProc0Reg = dest,
            CoProc0WriteBack = writeBack,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public unsafe static MipsExecution<T> CreateFloatWriteback<TFloat>(FloatRegister dest, TFloat writeBack)
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
            return new MipsExecution<T>
            {
                FloatReg = dest,
                FWordWriteBack = (int)longValue,
            };
        }
        else
        {
            return new MipsExecution<T>
            {
                FloatReg = dest,
                FLongWriteBack = longValue,
            };
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateMemRead(GPRegister dest, T address, int size, bool signed = true)
    {
        return new MipsExecution<T>
        {
            GPR = dest,
            MemAddress = address,
            MemSize = (uint)size,
            SideEffect = signed ? SideEffect.ReadMemorySigned : SideEffect.ReadMemory,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateMemWrite(T writeBack, T address, int size)
    {
        return new MipsExecution<T>
        {
            WriteBack = writeBack,
            MemAddress = address,
            MemSize = (uint)size,
            SideEffect = SideEffect.WriteMemory,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateJump(T absolutePC)
    {
        return new MipsExecution<T>
        {
            ProgramCounter = absolutePC,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateJumpAndLink(T absolutePC, T returnAddress, GPRegister raReg = GPRegister.ReturnAddress)
    {
        return new MipsExecution<T>
        {
            ProgramCounter = absolutePC,
            WriteBack = returnAddress,
            GPR = raReg,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateHighLow((T High, T Low) highLow)
    {
        return new MipsExecution<T>
        {
            High = highLow.High,
            Low = highLow.Low,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateLow(T low)
    {
        return new MipsExecution<T>
        {
            Low = low,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateHigh(T high)
    {
        return new MipsExecution<T>
        {
            High = high,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsExecution{T}"/> struct.
    /// </summary>
    public static MipsExecution<T> CreateEffect(SideEffect sideEffect)
    {
        return new MipsExecution<T>
        {
            SideEffect = sideEffect,
        };
    }

    /// <summary>
    /// Gets the writeback value to the selected GPR register.
    /// </summary>
    public T WriteBack { get; init; }

    /// <summary>
    /// Gets the general purpose register destination of the output.
    /// </summary>
    /// <remarks>
    /// <see cref="GPRegister.Zero"/> if none.
    /// </remarks>
    public GPRegister GPR { get; init; }

    /// <summary>
    /// Gets the type of secondary effect from the execution, if any.
    /// </summary>
    public SideEffect SideEffect { get; init; }

    /// <summary>
    /// Gets the new value of the low register if applicable.
    /// </summary>
    public T Low
    {
        get => _secondary1;
        init
        {
            _secondary1 = value;
            SideEffect = MergeHighLow(SideEffect.Low);
        }
    }

    /// <summary>
    /// Gets the new value of the low register if applicable.
    /// </summary>
    public T High
    {
        get => T.CreateTruncating(_secondary2);
        init
        {
            _secondary2 = ulong.CreateTruncating(value);
            SideEffect = MergeHighLow(SideEffect.High);
        }
    }

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
    /// Gets the register set to writeback to for co-process writeback.
    /// </summary>
    public GPRegister CoProcReg
    {
        get => (GPRegister)byte.CreateTruncating(BitField.GetField(_secondary1, REG_BITCOUNT, 0));
        init
        {
            BitField.SetField(ref _secondary1, REG_BITCOUNT, 0, T.CreateTruncating((byte)value));
            SideEffect = SideEffect.WriteCoProc0;
        }
    }

    /// <summary>
    /// Gets the coproc0 register for a co-process writeback.
    /// </summary>
    public CP0Registers CoProc0Reg
    {
        get => (CP0Registers)CoProcReg;
        init => CoProcReg = (GPRegister)value;
    }

    /// <summary>
    /// Gets the coproc1 register for a co-process writeback.
    /// </summary>
    public FloatRegister FloatReg
    {
        get => (FloatRegister)CoProcReg;
        init => CoProcReg = (GPRegister)value;
    }

    /// <summary>
    /// Gets the value writing back to co-processor0.
    /// </summary>
    public T CoProc0WriteBack
    {
        get => T.CreateTruncating(_secondary2);
        init
        {
            _secondary2 = ulong.CreateTruncating(value);
            SideEffect = SideEffect.WriteCoProc0;
        }
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="long"/>.
    /// </summary>
    public int FWordWriteBack
    {
        get => (int)FLongWriteBack;
        init
        {
            _secondary2 = (uint)value;
            SideEffect = SideEffect.WriteFloat;
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
            SideEffect = SideEffect.WriteDouble;
        }
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="float"/>.
    /// </summary>
    public float FFloatWriteBack
    {
        get => BitConverter.Int32BitsToSingle(FWordWriteBack);
        init => FWordWriteBack = BitConverter.SingleToInt32Bits(value);
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="double"/>.
    /// </summary>
    public double FDoubleWriteBack
    {
        get => BitConverter.Int64BitsToDouble(FLongWriteBack);
        init => FLongWriteBack = BitConverter.DoubleToInt64Bits(value);
    }

    /// <summary>
    /// Gets a value indicating whether or not execution handled the PC changing.
    /// </summary>
    public bool PCHandled => SideEffect is SideEffect.ProgramCounter;

    private SideEffect MergeHighLow(SideEffect @new)
    {
        if (SideEffect is SideEffect.Low or
            SideEffect.High or SideEffect.HighLow)
        {
            return SideEffect | @new;
        }

        return @new;
    }
}
