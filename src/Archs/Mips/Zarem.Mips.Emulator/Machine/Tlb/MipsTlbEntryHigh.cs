// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// Represents the upper mapping properties (virtual match tags and address filters)
/// for a page entry inside a MIPS TLB slot frame.
/// </summary>
/// <typeparam name="T">The underlying register type constraint (<see cref="uint"/> or <see cref="ulong"/>).</typeparam>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MipsTlbEntryHigh<T>
    where T : unmanaged, IBinaryInteger<T>
{
    private const int ASID_BIT_SIZE = 8;
    private const int ASID_BIT_OFFSET = 0;

    private const int VPN2_BIT_SIZE_32 = 19;
    private const int VPN2_BIT_SIZE_64 = 49;
    private const int VPN2_BIT_OFFSET = 13;

    private const int REGION_BIT_SIZE = 2;
    private const int REGION_BIT_OFFSET = 63;
    
    private T _value;

    /// <summary>
    /// Gets or sets the raw bitfield configuration for this EntryHi image.
    /// </summary>
    public T RawValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _value;
        set => _value = value;
    }

    /// <summary>
    /// Gets or sets the Address Space Identifier (ASID) used to filter matching address contexts.
    /// </summary>
    public byte AddressSpaceId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => byte.CreateTruncating(BitField.GetField(_value, ASID_BIT_SIZE, ASID_BIT_OFFSET));
        set => BitField.SetField(ref _value, ASID_BIT_SIZE, ASID_BIT_OFFSET, T.CreateTruncating(value));
    }

    /// <summary>
    /// Gets or sets the Virtual Page Number divided by 2 (VPN2) spanning bits 31-13 (32-bit) or bits 61-13 (64-bit).
    /// </summary>
    public T VirtualPageNumber2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => BitField.GetField(_value, Is64Bit ? VPN2_BIT_SIZE_64 : VPN2_BIT_SIZE_32, VPN2_BIT_OFFSET);
        set => BitField.SetField(ref _value, Is64Bit ? VPN2_BIT_SIZE_64 : VPN2_BIT_SIZE_32, VPN2_BIT_OFFSET, value);
    }

    /// <summary>
    /// Gets or sets the 2-bit Virtual Address Region (Bits 63-62) used exclusively in 64-bit addressing models.
    /// Returns 0 when executing in a 32-bit architecture constraint.
    /// </summary>
    public byte Region
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => Is64Bit
            ? byte.CreateTruncating(BitField.GetField(_value, REGION_BIT_SIZE, REGION_BIT_OFFSET))
            : (byte)0;
        set
        {
            if (Is64Bit)
            {
                BitField.SetField(ref _value, REGION_BIT_SIZE, REGION_BIT_OFFSET, T.CreateTruncating(value));
            }
        }
    }

    private static bool Is64Bit => sizeof(T) == sizeof(long);
}
