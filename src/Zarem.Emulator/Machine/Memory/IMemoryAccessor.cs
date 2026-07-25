// Avishai Dernis 2026

using System;
using System.IO;
using System.Numerics;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.Machine.Memory;

/// <summary>
/// An interface for a memory accessor in a computer, such as the physical bus or a virtual memory system.
/// </summary>
public interface IMemoryAccessor
{
    /// <summary>
    /// Attempts to read a value from virtual memory.
    /// </summary>
    /// <typeparam name="T">The unmanaged primitive type to read.</typeparam>
    /// <param name="address">The virtual address to read from.</param>
    /// <param name="value">When this method returns, contains the value read from memory if successful.</param>
    /// <returns>A <see cref="MemoryAccessResult"/> indicating the outcome of the operation.</returns>
    MemoryAccessResult TryRead<T>(ulong address, out T value) where T : unmanaged, IBinaryNumber<T>;

    /// <summary>
    /// Reads a <typeparamref name="T"/> from memory.
    /// </summary>
    /// <typeparam name="T">The type of object to read from memory.</typeparam>
    /// <param name="address">The address to read from.</param>
    /// <returns>The <typeparamref name="T"/> at <paramref name="address"/>.</returns>
    T Read<T>(ulong address) where T : unmanaged, IBinaryNumber<T>;

    /// <summary>
    /// Attempts to read a block of bytes from virtual memory into a destination buffer.
    /// </summary>
    /// <param name="address">The virtual address to start reading from.</param>
    /// <param name="buffer">The destination buffer to write the data into.</param>
    /// <returns>A <see cref="MemoryAccessResult"/> indicating the outcome of the operation.</returns>
    MemoryAccessResult TryRead(ulong address, Span<byte> buffer);

    /// <summary>
    /// Reads a span of bytes from memory.
    /// </summary>
    /// <param name="address">The address to read from.</param>
    /// <param name="buffer">The buffer to read the data into.</param>
    void Read(ulong address, Span<byte> buffer);

    /// <summary>
    /// Attempts to write a value to virtual memory.
    /// </summary>
    /// <typeparam name="T">The unmanaged primitive type to write.</typeparam>
    /// <param name="address">The virtual address to write to.</param>
    /// <param name="value">The value to write to memory.</param>
    /// <returns>A <see cref="MemoryAccessResult"/> indicating the outcome of the operation.</returns>
    MemoryAccessResult TryWrite<T>(ulong address, T value) where T : unmanaged, IBinaryNumber<T>;

    /// <summary>
    /// Write a <typeparamref name="T"/> to memory.
    /// </summary>
    /// <typeparam name="T">The type of object to write from memory.</typeparam>
    /// <param name="address">The address to write to.</param>
    /// <param name="value">The value to write to memory.</param>
    void Write<T>(ulong address, T value) where T : unmanaged, IBinaryNumber<T>;

    /// <summary>
    /// Attempts to write a block of bytes from a source buffer into virtual memory.
    /// </summary>
    /// <param name="address">The virtual address to start writing to.</param>
    /// <param name="buffer">The read-only source span containing the bytes to write.</param>
    /// <returns>A <see cref="MemoryAccessResult"/> indicating the outcome of the operation.</returns>
    MemoryAccessResult TryWrite(ulong address, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Write a span of bytes to memory.
    /// </summary>
    /// <param name="address">The address to write to.</param>
    /// <param name="buffer">The bytes to write to memory.</param>
    void Write(ulong address, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Gets the <see cref="IMemoryAccessor"/> as a <see cref="Stream"/>.
    /// </summary>
    Stream AsStream();
}
