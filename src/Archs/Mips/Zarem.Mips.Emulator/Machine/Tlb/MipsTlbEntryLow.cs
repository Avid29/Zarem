// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;

namespace Zarem.Mips.Emulator.Machine.Tlb;
/// <summary>
/// Represents the lower mapping properties (physical translation details and permissions) 
/// for a single page entry inside a MIPS TLB slot frame.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct MipsTlbEntryLow
{
    private const int GLOBAL_BIT_INDEX = 0;
    private const int VALID_BIT_INDEX = 1;
    private const int DIRTY_BIT_INDEX = 2;
    private const int CACHE_BIT_SIZE = 3;
    private const int CACHE_BIT_OFFSET = 3;
    private const int PFN_BIT_SIZE = 24;
    private const int PFN_BIT_OFFSET = 6;

    [FieldOffset(0)] private uint _value;

    /// <summary>
    /// Gets or sets the raw bitfield configuration for this EntryLo image.
    /// </summary>
    public uint RawValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _value;
        set => _value = value;
    }

    /// <summary>
    /// Gets or sets the physical Page Frame Number (PFN) for the memory translation target.
    /// </summary>
    public uint PFN
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetField(_value, PFN_BIT_SIZE, PFN_BIT_OFFSET);
        set => BitField.SetField(ref _value, PFN_BIT_SIZE, PFN_BIT_OFFSET, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this translation block is valid.
    /// </summary>
    public bool Valid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetBit(_value, VALID_BIT_INDEX);
        set => BitField.SetBit(ref _value, VALID_BIT_INDEX, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this page is dirty (writable).
    /// </summary>
    public bool Dirty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetBit(_value, DIRTY_BIT_INDEX);
        set => BitField.SetBit(ref _value, DIRTY_BIT_INDEX, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this specific entry ignores ASID processing matching filters.
    /// </summary>
    public bool Global
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetBit(_value, GLOBAL_BIT_INDEX);
        set => BitField.SetBit(ref _value, GLOBAL_BIT_INDEX, value);
    }
}
