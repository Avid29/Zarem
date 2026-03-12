// Avishai Dernis 2026

using System;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.Machine.Devices;

/// <summary>
/// An <see cref="IBusDevice"/> that is the system RAM.
/// </summary>
public class RamDevice : IBusDevice
{
    private readonly byte[][] _pageTable;
    private readonly uint _pageSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="RamDevice"/> class.
    /// </summary>
    public RamDevice(ulong size, uint pageSize = 4096)
    {
        BusRangeSize = size;
        _pageSize = pageSize;

        // Calculate total pages needed for the range
        _pageTable = new byte[(size + pageSize - 1) / pageSize][];
    }

    /// <inheritdoc/>
    public string Name => "RAM";

    /// <inheritdoc/>
    public ulong BusRangeSize { get; }

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        int remaining = destination.Length;
        int destOffset = 0;
        ulong currentAddr = offset;

        while (remaining > 0)
        {
            ulong pageIdx = currentAddr / _pageSize;
            int pageOffset = (int)(currentAddr % _pageSize);
            int bytesInPage = (int)Math.Min((ulong)_pageSize - (ulong)pageOffset, (ulong)remaining);

            var page = _pageTable[pageIdx];
            if (page == null)
            {
                // Sparse Read: If page doesn't exist, it's logically all zeros
                destination.Slice(destOffset, bytesInPage).Clear();
            }
            else
            {
                // Copy from the existing page
                page.AsSpan(pageOffset, bytesInPage).CopyTo(destination.Slice(destOffset, bytesInPage));
            }

            remaining -= bytesInPage;
            destOffset += bytesInPage;
            currentAddr += (ulong)bytesInPage;
        }
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        int remaining = source.Length;
        int srcOffset = 0;
        ulong currentAddr = offset;

        while (remaining > 0)
        {
            ulong pageIdx = currentAddr / _pageSize;
            int pageOffset = (int)(currentAddr % _pageSize);
            int bytesToCopy = (int)Math.Min((ulong)_pageSize - (ulong)pageOffset, (ulong)remaining);

            // Lazy Allocation: Create the page only when it's written to
            if (_pageTable[pageIdx] == null)
            {
                _pageTable[pageIdx] = new byte[_pageSize];
            }

            source.Slice(srcOffset, bytesToCopy).CopyTo(_pageTable[pageIdx].AsSpan(pageOffset, bytesToCopy));

            remaining -= bytesToCopy;
            srcOffset += bytesToCopy;
            currentAddr += (ulong)bytesToCopy;
        }
    }
}
