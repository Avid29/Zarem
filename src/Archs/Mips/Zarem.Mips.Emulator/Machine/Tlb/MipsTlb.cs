// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;

namespace Zarem.Mips.Emulator.Machine.Tlb;

/// <summary>
/// A hardware-accurate MIPS Translation Lookaside Buffer supporting variable page sizes
/// and dynamic 32-bit or 64-bit address space translation.
/// </summary>
/// <typeparam name="T">The underlying register type constraint (<see cref="uint"/> or <see cref="ulong"/>).</typeparam>
public unsafe class MipsTlb<T> : IMipsTlb
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly MipsTlbEntry<T>[] _slots;
    private readonly MipsCpu<T> _cpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsTlb{T}"/> class.
    /// </summary>
    public MipsTlb(MipsCpu<T> cpu, int slotCount = 64)
    {
        _cpu = cpu;
        _slots = new MipsTlbEntry<T>[slotCount];
    }

    /// <summary>
    /// Gets the underlying hardware entry slots contained within this buffer.
    /// </summary>
    public Span<MipsTlbEntry<T>> Slots => _slots;

    /// <inheritdoc/>
    public int SlotCount => _slots.Length;

    /// <inheritdoc/>
    public ulong Translate(ulong virtualAddress)
    {
        if (TryTranslate(virtualAddress, out var pAddress) is MemoryAccessResult.Success)
            return pAddress;

        // TODO: Improve exception type/message
        return ThrowHelper.ThrowArgumentOutOfRangeException<ulong>();
    }

    /// <inheritdoc/>
    public MemoryAccessResult TryTranslate(ulong virtualAddress, out ulong pAddress)
    {
        pAddress = default;

        if (!IsMapped(virtualAddress, out var isProtected))
        {
            if (isProtected && _cpu.CoProcessor0.ActingPrivilegeMode is not PrivilegeMode.Kernel)
                return MemoryAccessResult.AccessViolation;

            // Apply the fixed physical mask for MIPS32 / MIPS64 compatibility windows
            if ((virtualAddress >> 32) == 0 || virtualAddress >= 0xFFFF_FFFF_8000_0000UL)
            {
                pAddress = virtualAddress & 0x1FFF_FFFF;
            }
            else // XKPhys 64-bit segment
            {
                pAddress = virtualAddress & 0x07FF_FFFF_FFFF_FFFFUL;
            }

            pAddress = virtualAddress;
            return MemoryAccessResult.Success;
        }

        T vAddress = T.CreateTruncating(virtualAddress);

        int slotIndex = FindMatchingSlotIndex(vAddress);
        if (slotIndex < 0)
        {
            pAddress = 0;
            return MemoryAccessResult.TranslationFault;
        }

        ref readonly var slot = ref _slots[slotIndex];
        int oddPageBitIndex = GetOddPageBitIndex(slot.PageMask);
        bool isOddPage = IsOddPageAddress(vAddress, oddPageBitIndex);

        // Match the architecture fields provided by the user (Low0 / Low1)
        var selectedLo = isOddPage ? slot.Low1 : slot.Low0;

        if (!selectedLo.Valid)
        { 
            // Match found but page is invalid -> TLB Invalid Exception (TLBL/TLBS)
            pAddress = 0;
            return MemoryAccessResult.AccessViolation;
        }

        // Reconstruct physical destination address using the PFN and lower page offset bits
        ulong pfn = ulong.CreateTruncating(selectedLo.PageFrameNumber);
        ulong offsetMask = (1UL << oddPageBitIndex) - 1;
        ulong pageOffset = virtualAddress & offsetMask;

        // Mask out the lower offset bits from the PFN base allocation alignment, 
        // then combine with the page offset to compute the true physical pointer.
        ulong pfnBaseAddress = (pfn << 12) & ~offsetMask;

        pAddress = pfnBaseAddress | pageOffset;
        return MemoryAccessResult.Success;
    }


    /// <inheritdoc/>
    public int InitilizeSegment(int index, ulong virtualStartAddress, ulong size)
    {
        const ulong PageSize = 4096;
        const ulong DualPageBlockSize = PageSize * 2;

        // Align the starting address down to the nearest 8KB dual-page boundary
        ulong currentVAddr = virtualStartAddress & ~(DualPageBlockSize - 1);
        ulong endVAddr = virtualStartAddress + size;

        int i = index;
        while (currentVAddr < endVAddr)
        {
            if (i >= SlotCount)
            {
                ThrowHelper.ThrowInsufficientMemoryException();
            }

            // For this hosted environment, we can identity-map the virtual pages straight 
            // to physical allocations on the bus, or integrate a physical page pool allocator.
            ulong low0Paddr = currentVAddr;
            ulong low1Paddr = currentVAddr + PageSize;

            var slot = new MipsTlbEntry<T>
            {
                PageMask = T.Zero, // 0 standardizes to a 4KB sub-page size matching MIPS specifications

                // The VPN2 field encapsulates bits 63:13 of the dual-page block pointer
                Hi = new MipsTlbEntryHigh<T>
                {
                    VirtualPageNumber2 = T.CreateTruncating(currentVAddr >> 13),
                },

                Low0 = new MipsTlbEntryLow<T>
                {
                    Valid = true,
                    Dirty = true,                                       // Allow read and write permissions
                    Cache = MipsCacheAttribute.CacheableNoncoherent,    // Cacheable tracking
                    PageFrameNumber = T.CreateTruncating(low0Paddr >> 12),
                },
                Low1 = new MipsTlbEntryLow<T>
                {
                    Valid = true,
                    Dirty = true,
                    Cache = MipsCacheAttribute.CacheableNoncoherent,
                    PageFrameNumber = T.CreateTruncating(low1Paddr >> 12),
                }
            };

            // Commit the configured translation entry straight into the CPU's TLB structure
            Write(i, slot);

            currentVAddr += DualPageBlockSize;
            i++;
        }

        return i - index;
    }

    /// <summary>
    /// Reads an entry from the TLB array at the specified index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly MipsTlbEntry<T> Read(int index)
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
    public int Probe(in MipsTlbEntry<T> entry) => Probe(entry.Hi);

    /// <inheritdoc cref="Probe(in MipsTlbEntryHigh{T})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Probe(in MipsTlbEntryHigh<T> entryHi)
    {
        // Probe solely cares about matching the Virtual Address tag from EntryHi
        return FindMatchingSlotIndex(entryHi.RawValue);
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

    private static bool IsMapped(ulong vAddress, out bool isProtected)
    {
        isProtected = true;

        // If any of the top 32 bits are set, evaluate 64-bit space rules first
        if ((vAddress >> 32) != 0)
        {
            switch (vAddress >> 62)
            {
                // xkuseg
                case 0:
                    isProtected = false;
                    return true;  
                case 1: return true;  // xksseg
                case 2: return false; // xkphys
                case 3:
                    // If it's the 32-bit sign-extended compatibility space, 
                    // fall down to the switch statement below. Otherwise, xkseg is mapped.
                    if (vAddress >= 0xFFFF_FFFF_8000_0000UL)
                        break;
                    return true;
            }
        }

        if ((uint)vAddress < 0x8000_0000)
            isProtected = false;

        return (uint)vAddress switch
        {
            < 0x8000_0000 => true, // kuseg
            >= 0x8000_0000 and < 0xA000_0000 => false, // kseg0
            >= 0xA000_0000 and < 0xC000_0000 => false, // kseg1
            >= 0xC000_0000 and < 0xE000_0000 => true, // keg2
            >= 0xE000_0000 => true, // keg3
        };
    }
}
