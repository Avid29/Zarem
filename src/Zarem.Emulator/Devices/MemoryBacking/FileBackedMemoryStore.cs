// Avishai Dernis 2026

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace Zarem.Emulator.Devices.MemoryBacking;

/// <summary>
/// An implementation of <see cref="IMemoryBackingStore"/> that uses a file-backed memory model.
/// </summary>
public unsafe class FileBackedMemoryStore : IMemoryBackingStore
{
    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly byte* _safePointer;
    private readonly SafeMemoryMappedViewHandle? _viewHandle;

    /// <summary>
    /// Initalizes a new instance of the <see cref="FileBackedMemoryStore"/> class.
    /// </summary>
    public FileBackedMemoryStore(string filePath, ulong size, MemoryMappedFileAccess access = MemoryMappedFileAccess.ReadWrite)
    {
        Size = size;
        _mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, (long)size, access);
        _accessor = _mmf.CreateViewAccessor(0, (long)size, access);

        // Acquire an unmanaged pointer directly from the MMF view handle
        _viewHandle = _accessor.SafeMemoryMappedViewHandle;
        _viewHandle.AcquirePointer(ref _safePointer);
    }

    /// <inheritdoc/>
    public ulong Size { get; }

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination) => _accessor.ReadArray((long)offset, destination.ToArray(), 0, destination.Length);

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source) => _accessor.WriteArray((long)offset, source.ToArray(), 0, source.Length);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte* GetPointer(ulong offset) => _safePointer + offset;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_viewHandle != null && !_viewHandle.IsClosed)
        {
            _viewHandle.ReleasePointer();
            _viewHandle.Dispose();
        }

        _accessor.Dispose();
        _mmf.Dispose();
    }
}
