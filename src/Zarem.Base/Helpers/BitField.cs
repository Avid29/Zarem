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
    /// <param name="offset">The offset of the bitfield.</param>
    /// <param name="signExtend">Whether or not the sign extend.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static T GetField<T>(T value, int size, int offset, bool signExtend = false)
        where T : unmanaged, IBinaryInteger<T>
    {
        T extracted;

        if (offset is 0)
        {
            extracted = value & ((T.One << size) - T.One);
        }
        else if (size + offset == sizeof(T) * 8)
        {
            extracted = value >> offset;
        }
        else
        {
            extracted = (value >> offset) & ((T.One << size) - T.One);
        }

        return signExtend ? SignExtend(extracted, size) : extracted;
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
    /// Sign-extends a value from an arbitrary bit size to a standard signed integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SignExtend<T>(T value, int size)
        where T : unmanaged, IBinaryInteger<T>
    {
        var signMask = T.One << (size - 1);
        var valueMask = (T.One << size) - T.One;
        var isolatedValue = value & valueMask;
        return isolatedValue - ((isolatedValue & signMask) << 1);
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
