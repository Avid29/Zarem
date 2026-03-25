// Avishai Dernis 2024

using System.Runtime.CompilerServices;

namespace Zarem.Helpers;

/// <summary>
/// A class containing methods for masking a <see cref="uint"/>.
/// </summary>
public static class BitField
{
    /// <summary>
    /// Gets a section of a <see cref="uint"/>.
    /// </summary>
    /// <param name="value">The uint containing the bitfield.</param>
    /// <param name="size">The size of the bitfield to grab.</param>
    /// <param name="offset">The offset of the bitfield</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetField(uint value, int size, int offset)
    {
        if (offset is 0)
        {
            return value & ((1u << size) - 1);
        }
        else if (size + offset == sizeof(uint) * 8)
        {
            return value >> offset;
        }

        return (value >> offset) & ((1u << size) - 1);
    }

    /// <summary>
    /// Sets a section of a <see cref="uint"/>.
    /// </summary>
    /// <param name="target">The target bitfield.</param>
    /// <param name="size">The size of the bitfield.</param>
    /// <param name="offset">The offset of the bitfield.</param>
    /// <param name="value">The value to set in the bitfield.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetField(ref uint target, int size, int offset, uint value)
    {
        uint mask = ((1u << size) - 1) << offset;
        target = (target & ~mask) | ((value << offset) & mask);
    }

    /// <summary>
    /// Checks a single bit in a uint bitfield.
    /// </summary>
    /// <param name="value">The bitfield.</param>
    /// <param name="bit">The bit to check</param>
    /// <returns>Whether or not the bit is flagged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBit(uint value, int bit) => ((value >> bit) & 1) != 0;

    /// <summary>
    /// Sets a single bit in a uint bitfield.
    /// </summary>
    /// <param name="target">The target bitfield.</param>
    /// <param name="bit">The bit to <see langword="set"/>.</param>
    /// <param name="value">Whether or not the but is on.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(ref uint target, int bit, bool value)
    {
        uint mask = (uint)1 << bit;
        target = value
            ? target |= mask
            : target &= ~mask;
    }
}
