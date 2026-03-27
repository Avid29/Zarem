// Avishai Dernis 2024

using System.Numerics;
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
    public unsafe static T GetField<T>(T value, int size, int offset)
        where T : unmanaged, IBinaryInteger<T>
    {
        if (offset is 0)
        {
            return value & ((T.One << size) - T.One);
        }
        else if (size + offset == sizeof(T) * 8)
        {
            return value >> offset;
        }

        return (value >> offset) & ((T.One << size) - T.One);
    }

    /// <summary>
    /// Sets a section of a <see cref="uint"/>.
    /// </summary>
    /// <param name="target">The target bitfield.</param>
    /// <param name="size">The size of the bitfield.</param>
    /// <param name="offset">The offset of the bitfield.</param>
    /// <param name="value">The value to set in the bitfield.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetField<T>(ref T target, int size, int offset, T value)
        where T : unmanaged, IBinaryInteger<T>
    {
        T mask = ((T.One << size) - T.One) << offset;
        target = (target & ~mask) | ((value << offset) & mask);
    }

    /// <summary>
    /// Checks a single bit in a uint bitfield.
    /// </summary>
    /// <param name="value">The bitfield.</param>
    /// <param name="bit">The bit to check</param>
    /// <returns>Whether or not the bit is flagged.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetBit<T>(T value, int bit)
        where T : unmanaged, IBinaryInteger<T>
        => ((value >> bit) & T.One) != T.Zero;

    /// <summary>
    /// Sets a single bit in a uint bitfield.
    /// </summary>
    /// <param name="target">The target bitfield.</param>
    /// <param name="bit">The bit to <see langword="set"/>.</param>
    /// <param name="value">Whether or not the but is on.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit<T>(ref T target, int bit, bool value)
        where T : unmanaged, IBinaryInteger<T>
    {
        T mask = T.One << bit;
        target = value
            ? target |= mask
            : target &= ~mask;
    }
}
