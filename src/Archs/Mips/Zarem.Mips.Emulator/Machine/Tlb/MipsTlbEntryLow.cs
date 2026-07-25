// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;
using Zarem.Mips.Emulator.Machine.Enums;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// Represents the lower mapping properties (physical translation details and permissions) 
/// for a single page entry inside a MIPS TLB slot frame.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MipsTlbEntryLow<T>
    where T : unmanaged, IBinaryInteger<T>
{
    private const int GLOBAL_BIT_INDEX = 0;
    private const int VALID_BIT_INDEX = 1;
    private const int DIRTY_BIT_INDEX = 2;
    private const int CACHE_BIT_SIZE = 3;
    private const int CACHE_BIT_OFFSET = 3;
    private const int PFN_BIT_SIZE_32 = 24;
    private const int PFN_BIT_SIZE_64 = 49;
    private const int PFN_BIT_OFFSET = 6;

    private T _value;

    /// <summary>
    /// Gets or sets the raw bitfield configuration for this EntryLo image.
    /// </summary>
    public T RawValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _value;
        set => _value = value;
    }

    /// <summary>
    /// Gets or sets the physical Page Frame Number (PFN) for the memory translation target.
    /// </summary>
    public T PageFrameNumber
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetField(_value, Is64Bit ? PFN_BIT_SIZE_64 : PFN_BIT_SIZE_32, PFN_BIT_OFFSET);
        set => BitField.SetField(ref _value, Is64Bit ? PFN_BIT_SIZE_64 : PFN_BIT_SIZE_32, PFN_BIT_OFFSET, value);
    }

    /// <summary>
    /// Gets or sets the cache mode.
    /// </summary>
    public MipsCacheAttribute Cache
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (MipsCacheAttribute)byte.CreateTruncating(BitField.GetField(_value, CACHE_BIT_SIZE, CACHE_BIT_OFFSET));
        set => BitField.SetField(ref _value, CACHE_BIT_SIZE, CACHE_BIT_OFFSET, T.CreateTruncating((byte)value));
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

    private static bool Is64Bit => sizeof(T) == sizeof(long);

    /// <summary>
    /// Casts a <see cref="MipsTlbEntryLow{T}"/> to a <typeparamref name="T"/>.
    /// </summary>
    public static implicit operator T(MipsTlbEntryLow<T> value) => Unsafe.As<MipsTlbEntryLow<T>, T>(ref value);

    /// <summary>
    /// Casts a <typeparamref name="T"/> to a <see cref="MipsTlbEntryLow{T}"/>.
    /// </summary>
    public static explicit operator MipsTlbEntryLow<T>(T value) => Unsafe.As<T, MipsTlbEntryLow<T>>(ref value);
}
