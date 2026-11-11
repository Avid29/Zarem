// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models.Enums;
using Zarem.Mips.Emulator.Machine.Enums;

namespace Zarem.Mips.Emulator.Machine.Memory;

/// <summary>
/// An <see cref="IAddressTranslator"/> for handling MIPS fixed memory segmentation and TLB translation.
/// </summary>
public class MipsAddressTranslator<T> : IAddressTranslator
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly MipsCpu<T> _cpu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsAddressTranslator{T}"/> class.
    /// </summary>
    public MipsAddressTranslator(MipsCpu<T> cpu)
    {
        _cpu = cpu;
    }

    /// <inheritdoc/>
    public ulong Translate(ulong virtualAddress)
    {
        if (TryTranslate(virtualAddress, out var pAddress) is MemoryAccessResult.Success)
            return pAddress;

        // TODO: Improve exception type/message
        return ThrowHelper.ThrowArgumentOutOfRangeException<ulong>();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MemoryAccessResult TryTranslate(ulong virtualAddress, out ulong pAddress)
    {
        pAddress = default;

        // If the address does not require TLB translation, apply fixed segment translation rules
        if (!RequiresTlbTranslation(virtualAddress, out var isProtected))
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

            return MemoryAccessResult.Success;
        }

        // If the address requires TLB translation, delegate to the CPU's TLB
        return _cpu.Tlb.TryTranslate(virtualAddress, out pAddress);
    }

    private static bool RequiresTlbTranslation(ulong vAddress, out bool isProtected)
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

        // if in the kuseg range, it's not protected
        if ((uint)vAddress < 0x8000_0000)
            isProtected = false;

        // Determine if the address is mapped based on the MIPS memory segments
        return (uint)vAddress switch
        {
            < 0x8000_0000 => true, // kuseg
            >= 0x8000_0000 and < 0xA000_0000 => false, // kseg0
            >= 0xA000_0000 and < 0xC000_0000 => false, // kseg1
            >= 0xC000_0000 and < 0xE000_0000 => true, // kseg2
            >= 0xE000_0000 => true, // kseg3
        };
    }
}
