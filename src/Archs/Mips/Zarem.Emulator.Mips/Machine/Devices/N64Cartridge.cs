// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Emulator.Machine.Devices.Interfaces;

namespace Zarem.Emulator.Machine.Devices;

/// <summary>
/// A <see cref="IDevice"/> that behaves as a N64 game cartridge.
/// </summary>
public unsafe class N64Cartridge : IBusDeviceDirect
{
    private readonly byte* _ptr;
    private readonly ulong _size;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64Cartridge"/> class.
    /// </summary>
    public N64Cartridge(string z64FilePath)
    {
        byte[] fileData = File.ReadAllBytes(z64FilePath);
        _size = (ulong)fileData.Length;

        // Allocate unmanaged memory
        _ptr = (byte*)NativeMemory.Alloc((nuint)_size);

        // Copy file data into the allocated native block
        fixed (byte* src = fileData)
        {
            Buffer.MemoryCopy(src, _ptr, _size, _size);
        }
    }

    /// <inheritdoc/>
    public ulong BusRangeSize => _size;

    /// <inheritdoc/>
    public string Name => "N64 Cartridge";

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte* GetPointer(ulong offset)
    {
        Guard.IsLessThan(offset, _size);
        return _ptr + offset;
    }

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        Guard.IsLessThan(offset + (ulong)destination.Length, _size);
        byte* sourcePtr = _ptr + offset;
        var span = new ReadOnlySpan<byte>(sourcePtr, destination.Length);
        span.CopyTo(destination);
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        throw new NotSupportedException("Cartridge memory is read-only.");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        NativeMemory.Free(_ptr);
    }
}
