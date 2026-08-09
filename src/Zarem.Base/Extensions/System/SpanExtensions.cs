// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zarem.Extensions.System;

/// <summary>
/// A class containing extensions for <see cref="Span{T}"/>.
/// </summary>
public unsafe static class SpanExtensions
{
    /// <summary>
    /// Reads an unmanaged value of type <typeparamref name="T"/> from a byte span,
    /// swapping its byte order if the specified endianness differs from the host architecture.
    /// </summary>
    /// <typeparam name="T">The unmanaged value type to read.</typeparam>
    /// <param name="buffer">The read-only byte buffer containing the binary data.</param>
    /// <param name="littleEndian"><c>true</c> if the source data is little-endian; <c>false</c> if big-endian.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadEndianness<T>(this ReadOnlySpan<byte> buffer, bool littleEndian)
        where T : unmanaged
    {
        T value = MemoryMarshal.Read<T>(buffer);

        // If the host endianness doesn't match the emulation endianness, swap it.
        return littleEndian != BitConverter.IsLittleEndian
            ? ReverseEndianness(value)
            : value;
    }

    /// <summary>
    /// Writes an unmanaged value of type <typeparamref name="T"/> to a byte span,
    /// converting its byte order to match the specified target endianness.
    /// </summary>
    /// <typeparam name="T">The unmanaged value type to write (must be 1, 2, 4, or 8 bytes in size).</typeparam>
    /// <param name="buffer">The destination byte span where the value will be written.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="littleEndian"><c>true</c> to write in little-endian byte order; <c>false</c> for big-endian. Defaults to <c>false</c>.</param>
    /// <exception cref="NotSupportedException">Thrown when <typeparamref name="T"/> is not 1, 2, 4, or 8 bytes in size.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteEndianness<T>(this Span<byte> buffer, T value, bool littleEndian = false)
        where T : unmanaged
    {
        // If host matches target, just write raw bytes
        if (littleEndian == BitConverter.IsLittleEndian)
        {
            MemoryMarshal.Write(buffer, in value);
            return;
        }

        // No match. Change endianness before writing
        // The CLR optimizes this into a single path based on the caller's 'T'
        if (sizeof(T) == 1)
        {
            buffer[0] = Unsafe.As<T, byte>(ref value);
        }
        else if (sizeof(T) == 2)
        {
            ushort val = Unsafe.As<T, ushort>(ref value);
            if (BitConverter.IsLittleEndian)
                BinaryPrimitives.WriteUInt16BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, val);
        }
        else if (sizeof(T) == 4)
        {
            uint val = Unsafe.As<T, uint>(ref value);
            if (BitConverter.IsLittleEndian)
                BinaryPrimitives.WriteUInt32BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, val);
        }
        else if (sizeof(T) == 8)
        {
            ulong val = Unsafe.As<T, ulong>(ref value);
            if (BitConverter.IsLittleEndian)
                BinaryPrimitives.WriteUInt64BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, val);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Writes an unmanaged value of type <typeparamref name="T"/> to a byte span,
    /// converting its byte order to match the specified target endianness.
    /// </summary>
    /// <typeparam name="T">The unmanaged value type to write.</typeparam>
    /// <param name="buffer">The destination byte span where the value will be written.</param>
    /// <param name="length">The number of bytes to write from the value.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="littleEndian"><c>true</c> to write in little-endian byte order; <c>false</c> for big-endian. Defaults to <c>false</c>.</param>
    /// <exception cref="NotSupportedException">Thrown when <typeparamref name="T"/> is not 1, 2, 4, or 8 bytes in size.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteEndianness<T>(this Span<byte> buffer, int length, T value, bool littleEndian = false)
        where T : unmanaged
    {
        if (sizeof(T) == length && length is 1 or 2 or 4 or 8)
        {
            buffer.WriteEndianness(value, littleEndian);
            return;
        }

        // View the raw byte representation of 'value' in host memory.
        ReadOnlySpan<byte> source = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1));

        // Copy the desired source bytes to the desired region of the buffer
        source = BitConverter.IsLittleEndian
            ? source[..length]
            : source.Slice(sizeof(T) - length, length);
        Span<byte> target = buffer[..length];
        source.CopyTo(target);

        // If target endianness is does not match host endianness, reverse the payload bytes in-place
        if (littleEndian != BitConverter.IsLittleEndian)
        {
            target.Reverse();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ReverseEndianness<T>(T value)
        where T : unmanaged
    {
        // sizeof(T) is a JIT constant. No branches in the final assembly.
        if (sizeof(T) == 1)
            return value;

        if (sizeof(T) == 2)
        {
            // Reinterprets the bytes of T as a ushort without boxing or conversion logic.
            ushort val = Unsafe.As<T, ushort>(ref value);
            ushort swapped = BinaryPrimitives.ReverseEndianness(val);
            return Unsafe.As<ushort, T>(ref swapped);
        }

        if (sizeof(T) == 4)
        {
            uint val = Unsafe.As<T, uint>(ref value);
            uint swapped = BinaryPrimitives.ReverseEndianness(val);
            return Unsafe.As<uint, T>(ref swapped);
        }

        if (sizeof(T) == 8)
        {
            ulong val = Unsafe.As<T, ulong>(ref value);
            ulong swapped = BinaryPrimitives.ReverseEndianness(val);
            return Unsafe.As<ulong, T>(ref swapped);
        }

        throw new NotSupportedException($"Size {sizeof(T)} not supported for endianness swap.");
    }
}
