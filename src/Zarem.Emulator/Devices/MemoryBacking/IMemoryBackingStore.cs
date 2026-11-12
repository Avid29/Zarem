// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Devices.MemoryBacking;

/// <summary>
/// An interface for a memory backing store that provides methods for reading and writing data to a memory region.
/// </summary>
public unsafe interface IMemoryBackingStore : IDisposable
{
    /// <summary>
    /// Gets the size of the memory region in bytes.
    /// </summary>
    ulong Size { get; }

    /// <summary>
    /// Gets a pointer to the memory region at the specified offset.
    /// </summary>
    /// <param name="offset">The offset within the memory region.</param>
    /// <returns>A pointer to the requested address.</returns>
    byte* GetPointer(ulong offset);

    /// <summary>
    /// Reads data from the memory region into the provided span.
    /// </summary>
    void Read(ulong offset, Span<byte> destination);

    /// <summary>
    /// Writes data from the span into the memory region.
    /// </summary>
    void Write(ulong offset, ReadOnlySpan<byte> source);
}
