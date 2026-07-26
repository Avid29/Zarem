// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// Represents a hardware-accurate MIPS Translation Lookaside Buffer (TLB) entry 
/// configured dynamically for either 32-bit (MIPS I/II) or 64-bit (MIPS III/IV/64) architectures.
/// Maps a single virtual page pair to two distinct physical page frame numbers (PFN).
/// </summary>
/// <typeparam name="T">The underlying register type constraint (<see cref="uint"/> or <see cref="ulong"/>).</typeparam>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MipsTlbEntry<T>
    where T : unmanaged, IBinaryInteger<T>
{
    private const int ASID_BIT_SIZE = 8;
    private const int ASID_BIT_OFFSET = 0;
    private const int VPN2_BIT_SIZE_32 = 19;
    private const int VPN2_BIT_SIZE_64 = 49;
    private const int VPN2_BIT_OFFSET = 13;
    private const int REGION_BIT_SIZE = 2;
    private const int REGION_BIT_OFFSET = 63;
    private const int MASK_BIT_SIZE = 16;
    private const int MASK_BIT_OFFSET = 13;

    private MipsTlbEntryHigh<T> _entryHi;
    private MipsTlbEntryLow<T> _entryLo0;
    private MipsTlbEntryLow<T> _entryLo1;
    private T _pageMask;

    /// <summary>
    /// Gets or sets the translation match view properties for the virtual address space (EntryHi).
    /// </summary>
    public MipsTlbEntryHigh<T> Hi
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _entryHi;
        set => _entryHi = value;
    }
    /// <summary>
    /// Gets or sets the translation view properties for the even-numbered target page (EntryLo0).
    /// </summary>
    public MipsTlbEntryLow<T> Low0
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _entryLo0;
        set => _entryLo0 = value;
    }

    /// <summary>
    /// Gets or sets the translation view properties for the odd-numbered target page (EntryLo1).
    /// </summary>
    public MipsTlbEntryLow<T> Low1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _entryLo1;
        set => _entryLo1 = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the page matching rules ignore the explicit Address Space Identifier matching filters.
    /// Architecturally, a MIPS entry is global if the G bit is flagged in both EntryLo0 and EntryLo1 tracking images.
    /// </summary>
    public bool Global
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _entryLo0.Global && _entryLo1.Global;
        set
        {
            _entryLo0.Global = value;
            _entryLo1.Global = value;
        }
    }

    /// <summary>
    /// Gets or sets the variable page size bitmask allocation configuration.
    /// </summary>
    public T PageMask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetField(_pageMask, MASK_BIT_SIZE, MASK_BIT_OFFSET);
        set => BitField.SetField(ref _pageMask, MASK_BIT_SIZE, MASK_BIT_OFFSET, value);
    }

    private static bool Is64Bit => sizeof(T) == sizeof(long);
}
