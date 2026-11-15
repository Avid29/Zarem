// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Extensions.System;

namespace Zarem.Emulator.Extensions;

/// <summary>
/// A class containing static extension methods for <see cref="IMemoryBackingStore"/> related interfaces.
/// </summary>
public static unsafe class MemoryBackingStoreExtensions
{
    /// <summary>
    /// Reads a <typeparamref name="T"/> from the device.
    /// </summary>
    /// <typeparam name="T">The type of object to read.</typeparam>
    /// <param name="device">The device to read from.</param>
    /// <param name="offset">The offset to read from.</param>
    /// <param name="littleEndian"><c>true</c> if the source data is little-endian; <c>false</c> if big-endian.</param>
    /// <returns>The <typeparamref name="T"/> at <paramref name="offset"/>.</returns>
    public static T Read<T>(this IMemoryBackingStore device, ulong offset, bool littleEndian = false)
        where T : unmanaged, IBinaryNumber<T>
    {
        // TODO: Copyless optimization where possible
        int size = sizeof(T);
        var buffer = stackalloc byte[sizeof(T)];
        var span = new Span<byte>(buffer, size);
        device.Read(offset, span);
        return span.ReadEndianness<T>(littleEndian);
    }

    /// <summary>
    /// Write a <typeparamref name="T"/> to the device.
    /// </summary>
    /// <typeparam name="T">The type of object to read.</typeparam>
    /// <param name="device">The device to write to.</param>
    /// <param name="offset">The offset to write to.</param>
    /// <param name="value">The value to write to memory.</param>
    /// <param name="littleEndian"><c>true</c> if the source data is little-endian; <c>false</c> if big-endian.</param>
    /// <returns>The <typeparamref name="T"/> at <paramref name="offset"/>.</returns>
    public static void Write<T>(this IMemoryBackingStore device, ulong offset, T value, bool littleEndian = false)
        where T : unmanaged, IBinaryNumber<T>
    {
        // TODO: Direct write optimization where possible
        int size = sizeof(T);
        var buffer = stackalloc byte[sizeof(T)];
        var span = new Span<byte>(buffer, size);
        span.WriteEndianness(value, littleEndian);
        device.Write(offset, span);
    }
}
