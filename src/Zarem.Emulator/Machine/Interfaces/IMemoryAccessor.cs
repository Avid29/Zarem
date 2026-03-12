// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for RAM in a computer.
/// </summary>
public interface IMemoryAccessor
{
    /// <summary>
    /// Reads a <typeparamref name="T"/> from memory.
    /// </summary>
    /// <typeparam name="T">The type of object to read from memory.</typeparam>
    /// <param name="address">The address to read from.</param>
    /// <returns>The <typeparamref name="T"/> at <paramref name="address"/>.</returns>
    T Read<T>(ulong address) where T : unmanaged;

    /// <summary>
    /// Reads a span of bytes from memory.
    /// </summary>
    /// <param name="address">The address to read from.</param>
    /// <param name="buffer">The buffer to read the data into.</param>
    void Read(ulong address, Span<byte> buffer);

    /// <summary>
    /// Write a <typeparamref name="T"/> to memory.
    /// </summary>
    /// <typeparam name="T">The type of object to write from memory.</typeparam>
    /// <param name="address">The address to write to.</param>
    /// <param name="value">The value to write to memory.</param>
    void Write<T>(ulong address, T value) where T : unmanaged;

    /// <summary>
    /// Write a span of bytes to memory.
    /// </summary>
    /// <param name="address">The address to write to.</param>
    /// <param name="buffer">The bytes to write to memory.</param>
    void Write(ulong address, ReadOnlySpan<byte> buffer);
}
