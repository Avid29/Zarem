// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// A struct for the layout of a MIPS32 Translation Lookaside Buffer (TLB) entry.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct MipsTlbEntry32
{
    private const int ASID_BIT_SIZE = 8;
    private const int ASID_BIT_OFFSET = 0;
    private const int VPN2_BIT_SIZE = 19;
    private const int VPN2_BIT_OFFSET = 13;

    private const int MASK_BIT_SIZE = 16;
    private const int MASK_BIT_OFFSET = 13;

    [FieldOffset(0)] private uint _entryHi;
    [FieldOffset(4)] private MipsTlbEntryLow _entryLo0;
    [FieldOffset(8)] private MipsTlbEntryLow _entryLo1;
    [FieldOffset(12)] private uint _pageMask;

    /// <summary>
    /// Gets or sets the Address Space Identifier (ASID).
    /// </summary>
    /// <remarks>
    /// Matches the current process ID to prevent cross-process memory pollution.
    /// </remarks>
    public byte ASID
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (byte)BitField.GetField(_entryHi, ASID_BIT_SIZE, ASID_BIT_OFFSET);
        set => BitField.SetField(ref _entryHi, ASID_BIT_SIZE, ASID_BIT_OFFSET, value);
    }

    /// <summary>
    /// Gets or sets the Virtual Page Number divided by 2 (handles the paired page architecture).
    /// </summary>
    /// <remarks>
    /// Corresponds to bits 31-13 of the virtual memory space address layout.
    /// </remarks>
    public uint VPN2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetField(_entryHi, VPN2_BIT_SIZE, VPN2_BIT_OFFSET);
        set => BitField.SetField(ref _entryHi, VPN2_BIT_SIZE, VPN2_BIT_OFFSET, value);
    }

    /// <summary>
    /// Gets or sets the translation view configuration for the even page (EntryLo0).
    /// </summary>
    public MipsTlbEntryLow Low0
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _entryLo0;
        set => _entryLo0 = value;
    }

    /// <summary>
    /// Gets or sets the translation view configuration for the odd page (EntryLo1).
    /// </summary>
    public MipsTlbEntryLow Low1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _entryLo1;
        set => _entryLo1 = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the page matching rules ignore the specific <see cref="ASID"/>.
    /// Architecturally, a MIPS entry is global if the G bit is set in both EntryLo0 and EntryLo1.
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
    /// Gets or sets the variable page size bitmask configuration.
    /// </summary>
    public uint PageMask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetField(_pageMask, MASK_BIT_SIZE, MASK_BIT_OFFSET);
        set => BitField.SetField(ref _pageMask, MASK_BIT_SIZE, MASK_BIT_OFFSET, value);
    }
}
