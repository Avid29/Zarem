// Avishai Dernis 2025

using System.Runtime.CompilerServices;
using Zarem.Helpers;
using Zarem.Mips.Emulator.Machine.Enums;

namespace Zarem.Mips.Emulator.Machine.Registers;

/// <summary>
/// CoProcessor0 Cause register.
/// </summary>
/// <remarks>
/// Holds the reason for the last exception and interrupt pending state.
/// </remarks>
public struct CauseRegister
{
    private const int EXCEPTION_CODE_OFFSET = 2;
    private const int EXCEPTION_CODE_SIZE = 5;

    private const int PENDING_INTERUPTS_OFFSET = 8;
    private const int PENDING_INTERUPTS_SIZE = 8;

    private const int COPROCESSOR_EXCEPTION_SIZE = 2;
    private const int COPROCESSOR_EXCEPTION_OFFSET = 28;

    private const int BRANCH_DELAY_BIT = 31;

    private uint _cause;

    /// <summary>
    /// Gets or sets the trap code for the last exception.
    /// </summary>
    public MipsTrap ExecptionCode
    {
        readonly get => (MipsTrap)BitField.GetField(_cause, EXCEPTION_CODE_SIZE, EXCEPTION_CODE_OFFSET);
        set => BitField.SetField(ref _cause, EXCEPTION_CODE_SIZE, EXCEPTION_CODE_OFFSET, (uint)value);
    }

    /// <summary>
    /// Gets or sets the pending interupts.
    /// </summary>
    public byte PendingInterupts
    {
        readonly get => (byte)BitField.GetField(_cause, PENDING_INTERUPTS_SIZE, PENDING_INTERUPTS_OFFSET);
        set => BitField.SetField(ref _cause, PENDING_INTERUPTS_SIZE, PENDING_INTERUPTS_OFFSET, value);
    }
    
    /// <summary>
    /// Gets or sets the co-processor for the exception.
    /// </summary>
    public byte CoProcessorException
    {
        readonly get => (byte)BitField.GetField(_cause, COPROCESSOR_EXCEPTION_SIZE, COPROCESSOR_EXCEPTION_OFFSET);
        set => BitField.SetField(ref _cause, COPROCESSOR_EXCEPTION_SIZE, COPROCESSOR_EXCEPTION_OFFSET, value);
    }

    /// <summary>
    /// Gets or sets if the last exception occured in a branch delay slot.
    /// </summary>
    public bool IsBranchDelayed
    {
        readonly get => BitField.GetBit(_cause, BRANCH_DELAY_BIT);
        set => BitField.SetBit(ref _cause, BRANCH_DELAY_BIT, value);
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="CauseRegister"/>.
    /// </summary>
    public static unsafe explicit operator CauseRegister(uint value) => Unsafe.As<uint, CauseRegister>(ref value);

    /// <summary>
    /// Casts a <see cref="CauseRegister"/> to a <see cref="uint"/>.
    /// </summary>
    public static unsafe explicit operator uint(CauseRegister value) => Unsafe.As<CauseRegister, uint>(ref value);
}
