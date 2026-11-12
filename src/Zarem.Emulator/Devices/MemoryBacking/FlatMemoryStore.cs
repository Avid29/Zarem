// Avishai Dernis 2026

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zarem.Emulator.Devices.MemoryBacking;

/// <summary>
/// An implementation of <see cref="IMemoryBackingStore"/> that uses a flat memory model.
/// </summary>
public unsafe class FlatMemoryStore : IMemoryBackingStore
{
    private readonly void* _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlatMemoryStore"/> class.
    /// </summary>
    public FlatMemoryStore(ulong size)
    {
        Size = size;
        _buffer = NativeMemory.AllocZeroed((nuint)size);
    }

    /// <inheritdoc/>
    public ulong Size { get; }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte* GetPointer(ulong offset) => (byte*)_buffer + offset;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        fixed (byte* dest = destination)
        {
            Unsafe.CopyBlock(dest, (byte*)_buffer + offset, (uint)destination.Length);
        }
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        fixed (byte* src = source)
        {
            Unsafe.CopyBlock((byte*)_buffer + offset, src, (uint)source.Length);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => NativeMemory.Free(_buffer);
}
