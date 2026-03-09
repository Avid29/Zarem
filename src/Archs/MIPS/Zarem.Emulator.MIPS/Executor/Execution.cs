// Avishai Dernis 2025

using System;
using Zarem.Emulator.Executor.Enum;
using Zarem.Helpers;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Executor;

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
    public static Execution CreateBranch(int relativePC)
    {
        return new Execution
        {
            Branch = relativePC,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Execution"/> struct.
    /// </summary>
    public static Execution CreateBranchAndLink(int relativePC, uint returnAddress, GPRegister raReg = GPRegister.ReturnAddress)
    {
        return new Execution
        {
            Branch = relativePC,
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
            SideEffect = SideEffect.JumpProgramCounter;
        }
    }

    /// <summary>
    /// Gets the branch PC value, if application.
    /// </summary>
    public readonly int Branch
    {
        get => (int)_secondary1;
        init
        {
            _secondary1 = (uint)value;
            SideEffect = SideEffect.BranchProgramCounter;
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
        get => (GPRegister)UintMasking.GetShiftMask(_secondary2, REG_BITCOUNT, 0);
        init
        {
            UintMasking.SetShiftMask(ref _secondary2, REG_BITCOUNT, 0, (uint)value);
            SideEffect = SideEffect.WriteCoProc;
        }
    }

    /// <summary>
    /// Gets the coproc0 register for a co-process writeback.
    /// </summary>
    public readonly CP0Registers CoProc0Reg
    {
        get => (CP0Registers)UintMasking.GetShiftMask(_secondary2, REG_BITCOUNT, 0);
        init
        {
            UintMasking.SetShiftMask(ref _secondary2, REG_BITCOUNT, 0, (uint)value);
            CoProcRegisterSet = RegisterSet.CoProc0;
        }
    }

    /// <summary>
    /// Gets the coproc1 register for a co-process writeback.
    /// </summary>
    public readonly FloatRegister FloatReg
    {
        get => (FloatRegister)UintMasking.GetShiftMask(_secondary2, REG_BITCOUNT, 0);
        init
        {
            UintMasking.SetShiftMask(ref _secondary2, REG_BITCOUNT, 0, (uint)value);
            CoProcRegisterSet = RegisterSet.FloatingPoints;
        }
    }

    /// <summary>
    /// Gets the register set to writeback to for co-process writeback.
    /// </summary>
    public readonly RegisterSet CoProcRegisterSet
    {
        get => (RegisterSet)UintMasking.GetShiftMask(_secondary2, REGSET_BITCOUNT, REGSET_OFFSET);
        init
        {
            UintMasking.SetShiftMask(ref _secondary2, REGSET_BITCOUNT, REGSET_OFFSET, (uint)value);
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
    /// Gets a value indicating whether or not execution handled the PC changing.
    /// </summary>
    public readonly bool PCHandled => SideEffect == SideEffect.JumpProgramCounter;

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
