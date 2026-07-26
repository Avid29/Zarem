// Avishai Dernis 2024

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.Machine.Tlb;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors;

/// <summary>
/// A class representing the status/control coprocessor unit.
/// </summary>
public class CoProcessor0<T> : ICoProcessor0
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private const uint NORMAL_EXCEPTION_VECTOR = 0x8000_0180;
    private const uint BOOT_STRAPPING_EXCEPTION_VECTOR = 0xBFC0_0180;

    private readonly MipsTlb<T> _tlb;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoProcessor0{T}"/> class.
    /// </summary>
    public CoProcessor0(MipsTlb<T> tlb)
    {
        _tlb = tlb;
        RegisterFile = new(tlb.SlotCount);
    }

    /// <summary>
    /// Gets the coprocessor0's register file.
    /// </summary>
    public MipsCo0RegisterFile<T> RegisterFile { get; }

    /// <inheritdoc/>
    public PrivilegeMode ActingPrivilegeMode
        => RegisterFile.StatusRegister.ErrorLevel || RegisterFile.StatusRegister.ExceptionLevel
        ? PrivilegeMode.Kernel
        : RegisterFile.StatusRegister.PrivilegeMode;

    /// <inheritdoc/>
    public PrivilegeMode PrivilegeMode
    {
        get => RegisterFile.StatusRegister.PrivilegeMode;
        set
        {
            var status = RegisterFile.StatusRegister;
            status.PrivilegeMode = value;
            RegisterFile.StatusRegister = status;
        }
    }

    /// <summary>
    /// Gets the current exception vector.
    /// </summary>
    public T ExceptionVector => T.CreateTruncating(
        RegisterFile.StatusRegister.BootStrapping
        ? BOOT_STRAPPING_EXCEPTION_VECTOR
        : NORMAL_EXCEPTION_VECTOR);

    /// <summary>
    /// Gets or sets the value of a register on the coprocessor.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    public T this[CP0Registers reg]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => RegisterFile[(int)reg];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => RegisterFile[(int)reg] = value;
    }

    /// <summary>
    /// Handles entering a trap.
    /// </summary>
    public void EnterTrap(MipsTrap trap, T programCounter, bool isDelaySlot)
    {
        // Don't overwrite EPC if we are already in an exception (nested exception logic)
        if (!RegisterFile.StatusRegister.ExceptionLevel)
        {
            this[CP0Registers.ExceptionPC] = isDelaySlot
                ? programCounter - T.CreateTruncating(4)
                : programCounter;

            RegisterFile.StatusRegister = RegisterFile.StatusRegister with { ExceptionLevel = true };
        }

        RegisterFile.CauseRegister = RegisterFile.CauseRegister with
        {
            ExecptionCode = trap,
            IsBranchDelayed = isDelaySlot,
        };
    }

    /// <summary>
    /// Applies the writeback effect of a tlbp (Translation-Lookaside-Buffer Probe) instruction.
    /// </summary>
    public unsafe void WritebackTlbp()
    {
        int matchIndex = _tlb.Probe(RegisterFile.EntryHi);
        T index;
        if (matchIndex >= 0)
        {
            index = T.CreateTruncating(matchIndex);
        }
        else
        {
            index = T.One << ((sizeof(T) * 8) - 1);
        }

        this[CP0Registers.Index] = T.CreateTruncating(index);
    }

    /// <summary>
    /// Applies the writeback effect of a tlbr (Translation-Lookaside-Buffer Read) instruction.
    /// </summary>
    public void WritebackTlbr()
    {
        int targetIndex = int.CreateTruncating(this[CP0Registers.Index]) & 0x3F;
        ref readonly var entry = ref _tlb.Read(targetIndex);

        RegisterFile.EntryHi = entry.Hi;
        RegisterFile.EntryLow0 = entry.Low0;
        RegisterFile.EntryLow1 = entry.Low1;
        this[CP0Registers.PageMask] = entry.PageMask;
    }

    /// <summary>
    /// Applies the writeback effect of a tlbwi (Translation-Lookaside-Buffer Write Indexed) instruction.
    /// </summary>
    public void WritebackTlbwi()
    {
        // Assemble the entry
        MipsTlbEntry<T> entry = default;
        entry.Hi = RegisterFile.EntryHi;
        entry.Low0 = RegisterFile.EntryLow0;
        entry.Low1 = RegisterFile.EntryLow1;
        entry.PageMask = this[CP0Registers.PageMask];

        // Writeback to the TLB
        int targetIndex = int.CreateTruncating(this[CP0Registers.Index]) & 0x3F;
        _tlb.Write(targetIndex, in entry);
    }

    /// <summary>
    /// Applies the writeback effect of a tlbwr (Translation-Lookaside-Buffer Write Random) instruction.
    /// </summary>
    public void WritebackTlbwr()
    {
        // Assemble the entry
        MipsTlbEntry<T> entry = default;
        entry.Hi = RegisterFile.EntryHi;
        entry.Low0 = RegisterFile.EntryLow0;
        entry.Low1 = RegisterFile.EntryLow1;
        entry.PageMask = this[CP0Registers.PageMask];

        // Writeback to the TLB
        int targetIndex = int.CreateTruncating(this[CP0Registers.Random]) & 0x3F;
        _tlb.Write(targetIndex, in entry);
    }
}
