// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Emulator.Machine.Devices.Interfaces;

namespace Zarem.Emulator.Machine.Devices;

/// <summary>
/// An <see cref="IBusDevice"/> that is the system RAM.
/// </summary>
public unsafe class RamDevice : IBusDeviceDirect
{
    private readonly byte*[] _pageTable;
    private readonly uint _pageSize;
    private readonly uint _pageShift;

    /// <summary>
    /// Initializes a new instance of the <see cref="RamDevice"/> class.
    /// </summary>
    public RamDevice(ulong size, uint pageSize = 4096)
    {
        BusRangeSize = size;
        _pageSize = pageSize;
        _pageShift = (uint)BitOperations.TrailingZeroCount(pageSize);

        // Calculate total pages needed for the range
        _pageTable = new byte*[(size + pageSize - 1) / pageSize];
    }

    /// <inheritdoc/>
    public string Name => "RAM";

    /// <inheritdoc/>
    public ulong BusRangeSize { get; }

    /// <summary>
    /// Gets a pointer to an address within the ram device.
    /// </summary>
    /// <param name="offset">The offset within the device address range.</param>
    /// <returns>A pointer to the requested address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte* GetPointer(ulong offset)
    {
        ulong pageIdx = offset >> (int)_pageShift;
        uint pageOffset = (uint)(offset & (_pageSize - 1));

        byte* page = _pageTable[pageIdx];

        // Lazy Allocate on the fly
        if (page is null)
        {
            page = AllocatePage(pageIdx);
        }

        return page + pageOffset;
    }

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        int remaining = destination.Length;
        int destOffset = 0;
        ulong currentAddr = offset;

        fixed (byte* destPtr = destination)
        {
            while (remaining > 0)
            {
                ulong pageIdx = currentAddr >> (int)_pageShift;
                uint pageOffset = (uint)(currentAddr & (_pageSize - 1));

                int bytesInPage = (int)Math.Min(_pageSize - pageOffset, (uint)remaining);

                byte* page = _pageTable[pageIdx];
                if (page == null)
                {
                    Unsafe.InitBlock(destPtr + destOffset, 0, (uint)bytesInPage);
                }
                else
                {
                    Unsafe.CopyBlock(destPtr + destOffset, page + pageOffset, (uint)bytesInPage);
                }

                remaining -= bytesInPage;
                destOffset += bytesInPage;
                currentAddr += (ulong)bytesInPage;
            }
        }
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        int remaining = source.Length;
        int srcOffset = 0;
        ulong currentAddr = offset;

        fixed (byte* srcPtr = source)
        {
            while (remaining > 0)
            {
                ulong pageIdx = currentAddr >> (int)_pageShift;
                uint pageOffset = (uint)(currentAddr & (_pageSize - 1));

                int bytesToCopy = (int)Math.Min(_pageSize - pageOffset, (uint)remaining);

                byte* page = _pageTable[pageIdx];
                if (page == null)
                {
                    page = AllocatePage(pageIdx);
                }

                // Raw Memory Copy
                Unsafe.CopyBlock(page + pageOffset, srcPtr + srcOffset, (uint)bytesToCopy);

                remaining -= bytesToCopy;
                srcOffset += bytesToCopy;
                currentAddr += (ulong)bytesToCopy;
            }
        }
    }

    private byte* AllocatePage(ulong idx)
    {
        lock (_pageTable)
        {
            if (_pageTable[idx] is null)
            {
                // Allocate unmanaged memory so the GC doesn't move it
                _pageTable[idx] = (byte*)NativeMemory.AllocZeroed(_pageSize);
            }

            return _pageTable[idx];
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_pageTable)
        {
            foreach (var page in _pageTable)
            {
                if (page is not null)
                    NativeMemory.Free(page);
            }
        }
    }
}
