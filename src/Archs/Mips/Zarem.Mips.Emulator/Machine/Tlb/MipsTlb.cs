// Avishai Dernis 2026

using System;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.Memory;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// A hardware-accurate MIPS Translation Lookaside Buffer supporting variable page sizes
/// and dynamic 32-bit or 64-bit address space translation.
/// </summary>
/// <typeparam name="T">The underlying register type constraint (<see cref="uint"/> or <see cref="ulong"/>).</typeparam>
public unsafe class MipsTlb<T> : IAddressTranslator
    where T : unmanaged, IBinaryInteger<T>
{
    private readonly MipsTlbEntry<T>[] _slots;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsTlb{T}"/> class.
    /// </summary>
    /// <param name="slotCount"></param>
    public MipsTlb(int slotCount = 64)
    {
        _slots = new MipsTlbEntry<T>[slotCount];
    }

    /// <summary>
    /// Gets the underlying hardware entry slots contained within this buffer.
    /// </summary>
    public Span<MipsTlbEntry<T>> Slots => _slots;

    /// <inheritdoc/>
    public ulong Translate(ulong virtualAddress) => virtualAddress;

    /// <inheritdoc/>
    public bool TryTranslate(ulong virtualAddress, out ulong address)
    {
        T vAddress = T.CreateTruncating(virtualAddress);

        int slotIndex = FindMatchingSlotIndex(vAddress);
        if (slotIndex < 0)
        {
            address = 0;
            return false;
        }

        ref readonly var slot = ref _slots[slotIndex];
        int oddPageBitIndex = GetOddPageBitIndex(slot.PageMask);
        bool isOddPage = IsOddPageAddress(vAddress, oddPageBitIndex);

        // Match the architecture fields provided by the user (Low0 / Low1)
        var selectedLo = isOddPage ? slot.Low1 : slot.Low0;

        if (!selectedLo.Valid)
        {
            // Match found but page is invalid -> TLB Invalid Exception (TLBL/TLBS)
            address = 0;
            return false;
        }

        // Reconstruct physical destination address using the PFN and lower page offset bits
        ulong pfn = ulong.CreateTruncating(selectedLo.PageFrameNumber);
        ulong offsetMask = (1UL << oddPageBitIndex) - 1;
        ulong pageOffset = virtualAddress & offsetMask;

        // MIPS PFNs are shifted left by 12 to align into physical address spaces
        address = (pfn << 12) | pageOffset;
        return true;
    }

    /// <summary>
    /// Reads an entry from the TLB array at the specified index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref MipsTlbEntry<T> Read(int index)
    {
        int maskedIndex = index & (_slots.Length - 1);
        return ref _slots[maskedIndex];
    }

    /// <summary>
    /// Writes an entry into the TLB array at the specified index.
    /// Uses a read-only reference to completely avoid structural value copies.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int index, in MipsTlbEntry<T> entry)
    {
        int maskedIndex = index & (_slots.Length - 1);
        _slots[maskedIndex] = entry;
    }

    /// <summary>
    /// Searches the TLB slots for an entry that matches the virtual address property 
    /// configuration contained within the provided entry image.
    /// </summary>
    /// <returns>The index of the matching slot, or -1 if no match is found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Probe(in MipsTlbEntry<T> entry)
    {
        // Probe solely cares about matching the Virtual Address tag from EntryHi
        return FindMatchingSlotIndex(entry.Hi.RawValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindMatchingSlotIndex(T vAddress)
    {
        byte region = GetAddressRegion(vAddress);
        ReadOnlySpan<MipsTlbEntry<T>> localSlots = _slots;

        for (int i = 0; i < localSlots.Length; i++)
        {
            // TODO: Introduce caching to improve lookup times

            if (IsSlotMatch(in localSlots[i], vAddress, region))
                return i;
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSlotMatch(in MipsTlbEntry<T> slot, T vAddress, byte region)
    {
        // Check if the region matches (MIPS64 specific rule)
        if (sizeof(T) == sizeof(ulong) && slot.Hi.Region != region)
            return false;

        // Compute the dynamic page mask filter
        T combinedVpnMask = ~slot.PageMask;

        // Extract and compare the VPN2 components
        T incomingVpn2 = (vAddress >> 13) & combinedVpnMask;
        T slotVpn2 = slot.Hi.VirtualPageNumber2 & combinedVpnMask;

        if (incomingVpn2 != slotVpn2)
            return false;

        // TODO: Match ASID context checking here when context state is provided
        //if (!slot.Global && slot.Hi.AddressSpaceId != currentAsid)
        //    return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetAddressRegion(T vAddress)
    {
        // MIPS64 uses bits 63-62 for Region. MIPS32 ignores this (evaluates to 0).
        return (sizeof(T) == sizeof(long))
            ? byte.CreateTruncating(vAddress >> 62)
            : (byte)0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetOddPageBitIndex(T pageMask)
    {
        // Standard 4KB page uses bit 12 to distinguish between Low0 and Low1. 
        // Larger variable page masks shift this dynamic boundary index upward.
        return 12 + BitOperations.PopCount(ulong.CreateTruncating(pageMask));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOddPageAddress(T vAddress, int oddPageBitIndex)
    {
        return ((vAddress >> oddPageBitIndex) & T.One) == T.One;
    }
}
