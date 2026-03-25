// Avishai Dernis 2025

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Models.Enum;
using Zarem.Helpers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct representing the results of an instruction's execution.
/// </summary>
public readonly struct Execution
{
    private const int REG_BITCOUNT = 5;
    private const int REGSET_OFFSET = REG_BITCOUNT;
    private const int REGSET_BITCOUNT = 4;

    // These values are used for secondary effects
    // They can be (low, high), (memAddress, size*(-signed)), (pc, _), (writeback, register|regset)
    private readonly uint _secondary1;
    private readonly uint _secondary2;
    private readonly long _floatWriteback;

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateWriteback(GPRegister dest, uint writeBack)
    {
        return new Execution
        {
            GPR = dest,
            WriteBack = writeBack,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateWriteback(CP0Registers dest, uint writeBack)
    {
        return new Execution
        {
            CoProc0Reg = dest,
            CoProcWriteBack = writeBack,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateFloatWriteback<T>(FloatRegister dest, T writeBack)
        where T : INumber<T>
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

        if (Unsafe.SizeOf<T>() == sizeof(float))
        {
            return new Execution
            {
                FloatReg = dest,
                FWordWriteBack = (int)longValue,
            };
        }
        else
        {
            return new Execution
            {
                FloatReg = dest,
                FLongWriteBack = longValue,
            };
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateMemRead(GPRegister dest, uint address, int size, bool signed = true)
    {
        return new Execution
        {
            GPR = dest,
            MemAddress = address,
            MemSize = (uint)size,
            MemSigned = signed,
            SideEffect = SideEffect.ReadMemory,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateMemWrite(uint writeBack, uint address, int size)
    {
        return new Execution
        {
            WriteBack = writeBack,
            MemAddress = address,
            MemSize = (uint)size,
            SideEffect = SideEffect.WriteMemory,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateJump(uint absolutePC)
    {
        return new Execution
        {
            ProgramCounter = absolutePC,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateJumpAndLink(uint absolutePC, uint returnAddress, GPRegister raReg = GPRegister.ReturnAddress)
    {
        return new Execution
        {
            ProgramCounter = absolutePC,
            WriteBack = returnAddress,
            GPR = raReg,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateHighLow(ulong highLow)
    {
        return new Execution
        {
            High = (uint)(highLow >> 32),
            Low = (uint)highLow,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateHighLow((uint High, uint Low) highLow)
    {
        return new Execution
        {
            High = highLow.High,
            Low = highLow.Low,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateLow(uint low)
    {
        return new Execution
        {
            Low = low,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateHigh(uint high)
    {
        return new Execution
        {
            High = high,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateEffect(SideEffect sideEffect)
    {
        return new Execution
        {
            SideEffect = sideEffect,
        };
    }

    /// <summary>
    /// Gets the writeback value to the selected GPR register.
    /// </summary>
    public readonly uint WriteBack { get; init; }

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
    public readonly uint Low
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
    public readonly uint High
    {
        get => _secondary2;
        init
        {
            _secondary2 = value;
            SideEffect = MergeHighLow(SideEffect.High);
        }
    }

    /// <summary>
    /// Gets the new PC value, if application.
    /// </summary>
    public readonly uint ProgramCounter
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
    public readonly uint MemAddress
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
    public readonly uint MemSize
    {
        get => (uint)int.Abs((int)_secondary2);
        init => _secondary2 = (uint)(value * (MemSigned ? -1 : 1));
    }

    /// <summary>
    /// Gets whether or not the memory operation is singed (should sign-extend)
    /// </summary>
    public readonly bool MemSigned
    {
        get => (int)_secondary2 < 0;
        init => _secondary2 = (uint)(value != MemSigned ? -_secondary2 : _secondary2);
    }

    /// <summary>
    /// Gets the register set to writeback to for co-process writeback.
    /// </summary>
    public readonly GPRegister CoProcReg
    {
        get => (GPRegister)BitField.GetField(_secondary2, REG_BITCOUNT, 0);
        init
        {
            BitField.SetField(ref _secondary2, REG_BITCOUNT, 0, (uint)value);
            SideEffect = SideEffect.WriteCoProc;
        }
    }

    /// <summary>
    /// Gets the coproc0 register for a co-process writeback.
    /// </summary>
    public readonly CP0Registers CoProc0Reg
    {
        get => (CP0Registers)BitField.GetField(_secondary2, REG_BITCOUNT, 0);
        init
        {
            BitField.SetField(ref _secondary2, REG_BITCOUNT, 0, (uint)value);
            CoProcRegisterSet = RegisterSet.CoProc0;
        }
    }

    /// <summary>
    /// Gets the coproc1 register for a co-process writeback.
    /// </summary>
    public readonly FloatRegister FloatReg
    {
        get => (FloatRegister)BitField.GetField(_secondary2, REG_BITCOUNT, 0);
        init
        {
            BitField.SetField(ref _secondary2, REG_BITCOUNT, 0, (uint)value);
            CoProcRegisterSet = RegisterSet.FloatingPoints;
        }
    }

    /// <summary>
    /// Gets the register set to writeback to for co-process writeback.
    /// </summary>
    public readonly RegisterSet CoProcRegisterSet
    {
        get => (RegisterSet)BitField.GetField(_secondary2, REGSET_BITCOUNT, REGSET_OFFSET);
        init
        {
            BitField.SetField(ref _secondary2, REGSET_BITCOUNT, REGSET_OFFSET, (uint)value);
            SideEffect = SideEffect.WriteCoProc;
        }
    }

    /// <summary>
    /// Gets the value writing back to a co-processor.
    /// </summary>
    public readonly uint CoProcWriteBack
    {
        get => _secondary1;
        init
        {
            _secondary1 = value;
            SideEffect = SideEffect.WriteCoProc;
        }
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="long"/>.
    /// </summary>
    public readonly int FWordWriteBack
    {
        get => (int)FLongWriteBack;
        init
        {
            _floatWriteback = value;
            SideEffect = SideEffect.WriteFloat;
        }
    }

    /// <summary>
    /// Gets the value being written to the float processor  as a <see cref="long"/>.
    /// </summary>
    public readonly long FLongWriteBack
    {
        get => _floatWriteback;
        init
        {
            _floatWriteback = value;
            SideEffect = SideEffect.WriteDouble;
        }
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="float"/>.
    /// </summary>
    public readonly float FFloatWriteBack
    {
        get => BitConverter.Int32BitsToSingle(FWordWriteBack);
        init => FWordWriteBack = BitConverter.SingleToInt32Bits(value);
    }

    /// <summary>
    /// Gets the value being written to the float processor as a <see cref="double"/>.
    /// </summary>
    public readonly double FDoubleWriteBack
    {
        get => BitConverter.Int64BitsToDouble(FLongWriteBack);
        init => FLongWriteBack = BitConverter.DoubleToInt64Bits(value);
    }

    /// <summary>
    /// Gets a value indicating whether or not execution handled the PC changing.
    /// </summary>
    public readonly bool PCHandled => SideEffect is SideEffect.ProgramCounter;

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
