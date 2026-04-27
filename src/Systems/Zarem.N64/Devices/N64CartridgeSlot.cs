// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Emulator.Machine.Devices.Interfaces;

namespace Zarem.N64.Devices;

/// <summary>
/// A <see cref="IDevice"/> that behaves as a N64 game cartridge.
/// </summary>
public unsafe class N64CartridgeSlot : IBusDeviceDirect
{
    private byte* _ptr;
    private ulong _size;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64CartridgeSlot"/> class.
    /// </summary>
    public N64CartridgeSlot(ulong maxSize)
    {
        UnloadCartridge();

        BusRangeSize = maxSize;
    }

    /// <inheritdoc/>
    public ulong BusRangeSize { get; }

    /// <inheritdoc/>
    public string Name => "N64 Cartridge";

    /// <summary>
    /// Loads a z64 file as an N64 cartridge.
    /// </summary>
    public void LoadCartridge(string z64FilePath)
    {
        // Unload the current cartidge
        UnloadCartridge();

        // Load the file data
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

    /// <summary>
    /// Unloads the current z64 cartridge.
    /// </summary>
    public void UnloadCartridge()
    {
        if (_ptr is not null)
        {
            NativeMemory.Free(_ptr);
            _ptr = null;
            _size = 0;
        }
    }

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
        UnloadCartridge();
    }
}
