// Avishai Dernis 2024

namespace Zarem.Helpers;

/// <summary>
/// A class containing methods for masking a <see cref="uint"/>.
/// </summary>
public static class UintMasking
{
    /// <summary>
    /// Gets a section of a <see cref="uint"/>.
    /// </summary>
    /// <param name="value">The uint containing the bitfield.</param>
    /// <param name="size">The size of the bitfield to grab.</param>
    /// <param name="offset">The offset of the bitfield</param>
    /// <returns></returns>
    public static uint GetShiftMask(uint value, int size, int offset)
    {
        // Generate the mask by taking 2^(size) and subtracting one
        uint mask = (uint)(1 << size) - 1;

        // Shift right by the offset then mask off the size
        return mask & (value >> offset);
    }

    /// <summary>
    /// Sets a section of a <see cref="uint"/>.
    /// </summary>
    /// <param name="target">The target bitfield.</param>
    /// <param name="size">The size of the bitfield.</param>
    /// <param name="offset">The offset of the bitfield.</param>
    /// <param name="value">The value to set in the bitfield.</param>
    public static void SetShiftMask(ref uint target, int size, int offset, uint value)
    {
        // Generate the value mask by taking 2^(size),
        // subtracting one, and shifting.
        uint vMask = (uint)(1 << size) - 1;
        vMask <<= offset;

        // Then make the value mask and inverting
        // the instruction mask
        uint iMask = ~(vMask);

        // Mask the instruction and the value
        uint vMasked = (value << offset) & vMask;
        uint iMasked = target & iMask;

        // Combine the instruction and the value
        // post masking
        target = iMasked | vMasked;
    }

    /// <summary>
    /// Checks a single bit in a uint bitfield.
    /// </summary>
    /// <param name="value">The bitfield.</param>
    /// <param name="bit">The bit to check</param>
    /// <returns>Whether or not the bit is flagged.</returns>
    public static bool CheckBit(uint value, int bit) => ((value >> bit) & 1) != 0;

    /// <summary>
    /// Sets a single bit in a uint bitfield.
    /// </summary>
    /// <param name="target">The target bitfield.</param>
    /// <param name="bit">The bit to <see langword="set"/>.</param>
    /// <param name="value">Whether or not the but is on.</param>
    public static void SetBit(ref uint target, int bit, bool value)
    {
        uint mask = (uint)1 << bit;
        if (value)
        {
            target |= mask;
        }
        else
        {
            target &= ~mask;
        }
    }
}
